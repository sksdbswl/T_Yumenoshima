#if UNITY_EDITOR
using System;
using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace REIW
{
    public static class GitHooksInstaller
    {
        // 프로젝트별 1회 실행 여부를 기록할 센티넬 경로 (VCS에 굳이 올릴 필요 없음)
        private static readonly string SentinelPath = Path.Combine("ProjectSettings", "GitHooks.installed");

        // 설정하려는 include.path 값
        private const string TargetIncludePath = "../.gitconfig";

        [InitializeOnLoadMethod]
        private static void InstallOnce()
        {
            // 배치모드(빌드 머신) 등에서는 불필요 → 필요 시 제거하세요.
            if (Application.isBatchMode) return;

            // 이미 설치 이력 있으면 스킵
            if (File.Exists(SentinelPath))
                return;

            // Git 리포지토리인지 확인
            if (!Directory.Exists(Path.Combine(Environment.CurrentDirectory, ".git")))
            {
                UnityEngine.Debug.Log("[GitHooksInstaller] .git 폴더가 없어 건너뜁니다.");
                return;
            }

            try
            {
                // 이미 include.path에 들어있는지 확인
                if (!IsIncludeAlreadySet(TargetIncludePath))
                {
                    // 없으면 추가
                    RunGit($"config --local --add include.path \"{TargetIncludePath}\"");
                    UnityEngine.Debug.Log($"[GitHooksInstaller] git config include.path에 '{TargetIncludePath}' 추가 완료");
                }
                else
                {
                    UnityEngine.Debug.Log($"[GitHooksInstaller] 이미 include.path에 '{TargetIncludePath}'가 설정되어 있습니다.");
                }

                // 센티넬 생성 (성공 시에만)
                Directory.CreateDirectory(Path.GetDirectoryName(SentinelPath)!);
                File.WriteAllText(SentinelPath, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogWarning(
                    $"[GitHooksInstaller] 설치 중 예외 발생: {ex.Message}\n계속 반복되면 ProjectSettings/{Path.GetFileName(SentinelPath)} 파일을 수동 생성해 임시 회피 가능");
            }
        }

        private static bool IsIncludeAlreadySet(string value)
        {
            // --includes: 상속된 구성도 포함해 조회, 중복 방지
            string output = RunGitAndGetOutput("config --local --includes --get-all include.path");
            if (string.IsNullOrEmpty(output)) return false;

            foreach (var line in output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                // Git은 경로 슬래시/백슬래시가 섞일 수 있으므로 정규화
                var norm = NormalizePath(line.Trim());
                if (string.Equals(norm, NormalizePath(value), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string NormalizePath(string p)
        {
            return p.Replace('\\', '/').Trim().Trim('"', '\'');
        }

        private static void RunGit(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = false,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var p = Process.Start(psi);
            if (p == null) throw new Exception("git 프로세스를 시작할 수 없습니다.");
            p.WaitForExit();

            if (p.ExitCode != 0)
            {
                string err = p.StandardError.ReadToEnd();
                throw new Exception($"git {args} 실패 (ExitCode {p.ExitCode}): {err}");
            }
        }

        private static string RunGitAndGetOutput(string args)
        {
            var psi = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = args,
                WorkingDirectory = Environment.CurrentDirectory,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
            };

            using var p = Process.Start(psi);
            if (p == null) throw new Exception("git 프로세스를 시작할 수 없습니다.");
            string stdout = p.StandardOutput.ReadToEnd();
            string stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();

            if (p.ExitCode != 0)
                throw new Exception($"git {args} 실패 (ExitCode {p.ExitCode}): {stderr}");

            return stdout;
        }
    }
}
#endif
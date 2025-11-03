
namespace REIW.Animations
{
    public class AnimationEventListener : ICharacterEventListener
    {
        private bool _isRegisted;

        public AnimationEventListener(CharacterBaseEventBus eventBus)
        {
            Register(eventBus);
        }

        public void Register(CharacterBaseEventBus InEventBus)
        {
            if (_isRegisted)
                return;

            if (InEventBus != null)
            {
                InEventBus.Register(this);
                _isRegisted = true;
            }
        }

        public void Unregister(CharacterBaseEventBus InEventBus)
        {
            if (_isRegisted && InEventBus != null)
            {
                InEventBus.Unregister(this);
                _isRegisted = false;
            }
        }
    }
}

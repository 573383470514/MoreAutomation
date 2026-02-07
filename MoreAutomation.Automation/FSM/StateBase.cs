namespace MoreAutomation.Automation.FSM
{
    public abstract class StateBase
    {
        public abstract string Name { get; }
        public virtual void OnEnter() { }
        public virtual void OnUpdate() { }
        public virtual void OnExit() { }
    }
}
using System;

namespace DataInterfaceConsoleTest.Examples
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    sealed class CallableExMethodAttribute : Attribute
    {
        readonly bool enabled;
        private readonly InvokeKind invKind;

        public CallableExMethodAttribute(bool enabled, InvokeKind whenToCall)
        {
            this.enabled = enabled;
            this.invKind = whenToCall;
        }

        public bool Enabled => this.enabled && this.invKind != InvokeKind.None;
        public InvokeKind WhenToCall => this.invKind;


        // TODO ADD GAMESTATE CHANGED
        [Flags]
        public enum InvokeKind
        {
            None = 0,
            Startup = 1, // runs the tagged method when this program starts
            MatchStart = 2, // runs when a match is started, and the boards are being shown
            MatchExited = 4, // runs when a match is exited from

            TurnChange = 8, // runs when the turn changes, meaning that this runs once, when someone hits "Submit"
            BoardCountChanged = 16, // runs when the boardCount changes


            TurnChangedOrStartup = Startup | TurnChange, // on startup and on turn changed
            BoardCountChangedOrStartup = Startup | BoardCountChanged,

            All = Startup | MatchStart | MatchExited | TurnChange | BoardCountChanged
        }
    }
}

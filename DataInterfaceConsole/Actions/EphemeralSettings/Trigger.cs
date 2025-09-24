using System;

namespace DataInterfaceConsole.Actions.EphemeralSettings;
public class Trigger {
    private readonly Action action;

    public override string ToString() {
        return "Trigger";
    }

    public Trigger(Action action) {
        this.action = action;
    }

    public void Run() => action.Invoke();
}

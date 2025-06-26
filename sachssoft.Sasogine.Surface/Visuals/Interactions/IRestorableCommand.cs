namespace sachssoft.Sasogine.Surface.Visuals.Interactions;

public interface IRestorableCommandCore
{
    void Execute(params object[] args);

    void Unexecute(params object[] args);

}

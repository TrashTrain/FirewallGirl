
public class SequenceTurn
{

    private int sequenceCheck = 1;
    private bool virusAction = true;

    public static SequenceTurn instance = new SequenceTurn();

    public int GetSequenceCheck()
    {
        return sequenceCheck;
    }

    public void SetPlusSequenceCheck()
    {
        sequenceCheck++;
    }

    public void SetResetSequenceCheck()
    {
        sequenceCheck = 1;
    }

    public bool GetVirusAction()
    {
        return virusAction;
    }
    public bool SetVirusActionChange()
    {
        return virusAction = !virusAction;
    }
}

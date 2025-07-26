
public class SequenceTurn
{

    private int sequenceCheck = 1;

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
}

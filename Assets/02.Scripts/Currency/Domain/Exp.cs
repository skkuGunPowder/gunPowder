
public class Exp
{
    private int _value;

    public Exp(int value)
    {
        if (value < 0)
        {
            throw new System.Exception("경험치는 0 이상이어야 합니다.");
        }
        _value = value;
    }

    public int GetValue()
    {
        return _value;
    }

    public void Add(int value)
    {
        if (value < 0)
        {
            throw new System.Exception("추가할 경험치는 0 이상이어야 합니다.");
        }
        _value += value;
    }
}

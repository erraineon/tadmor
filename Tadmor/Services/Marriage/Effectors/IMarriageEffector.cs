namespace Tadmor.Services.Marriage
{
    public interface IMarriageEffector<TValue>
    {
        TValue GetNewValue(TValue current, TValue seed, MarriedCouple couple);
    }

    public interface IMarriageEffector
    {
        void Execute(MarriedCouple couple);
    }
}
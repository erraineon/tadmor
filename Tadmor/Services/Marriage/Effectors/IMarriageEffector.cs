namespace Tadmor.Services.Marriage
{
    public interface IMarriageEffector<TValue>
    {
        TValue GetNewValue(TValue current, TValue seed, MarriedCouple couple);
    }
}
namespace Runtime.Types
{
    [System.Flags]
    public enum HandRaiseType
    {
        None = 0,
        LeftHandRaised = 1 << 0,
        RightHandRaised = 1 << 2,
        LeftHandBelow = 1 << 3,
        RightHandBelow = 1 << 4,


        HandsRaised = LeftHandRaised | RightHandRaised,
        HandsBelow = LeftHandBelow | RightHandBelow,
    }
}
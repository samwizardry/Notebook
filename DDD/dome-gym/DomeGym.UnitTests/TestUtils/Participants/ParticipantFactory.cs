using DomeGym.Domain;
using DomeGym.UnitTests.TestConstants;

namespace DomeGym.UnitTests.TestUtils.Participants;

public static class ParticipantFactory
{
    public static Participant CreateParticipant(Guid? userId = null,
        Guid? id = null)
    {
        return new Participant(
            userId: userId ?? Constants.User.Id,
            id: id ?? Constants.Participant.Id);
    }
}

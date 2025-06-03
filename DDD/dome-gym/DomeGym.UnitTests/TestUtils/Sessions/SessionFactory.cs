using DomeGym.Domain;
using DomeGym.UnitTests.TestConstants;

namespace DomeGym.UnitTests.TestUtils.Sessions;

public static class SessionFactory
{
    public static Session CreateSession(int maxParticipants)
    {
        return new Session(
            maxParticipants: maxParticipants,
            trainerId: Constants.Trainer.Id,
            id: Constants.Session.Id);
    }
}

using DomeGym.UnitTests.TestUtils.Participants;
using DomeGym.UnitTests.TestUtils.Sessions;

using FluentAssertions;

namespace DomeGym.UnitTests;

public class SessionTests
{
    [Fact]
    public void ReserveSpot_WhenNoMoreRoom_ShouldFailReservation()
    {
        // Arrange
        var session = SessionFactory.CreateSession(maxParticipants: 1);
        var participant1 = ParticipantFactory.CreateParticipant(id: Guid.NewGuid());
        var participant2 = ParticipantFactory.CreateParticipant(id: Guid.NewGuid());

        // Act
        session.ReserveSpot(participant1);
        var action = () => session.ReserveSpot(participant2);

        // Assert
        action.Should().ThrowExactly<Exception>();
    }
}

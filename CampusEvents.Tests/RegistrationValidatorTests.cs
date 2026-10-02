using CampusEvents.Backend;
using Moq;
using Xunit;

namespace CampusEvents.Tests;

public class RegistrationValidatorTests
{
    private static RegistrationValidator Make(Mock<IEventRepository>? repo = null)
        => new((repo ?? new Mock<IEventRepository>()).Object);

    [Theory]
    [InlineData("juan@univ.edu.ph")]
    [InlineData("JUAN.DELACRUZ@UNIV.EDU.PH")]
    [InlineData("  maria@univ.edu.ph  ")]
    public void IsValidStudentEmail_AcceptsUniversityDomain(string email)
        => Assert.True(Make().IsValidStudentEmail(email));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("@univ.edu.ph")]
    [InlineData("juan@gmail.com")]
    [InlineData("juan@univ.edu.ph.evil.com")]
    [InlineData("a@b@univ.edu.ph")]
    [InlineData("ju an@univ.edu.ph")]
    public void IsValidStudentEmail_RejectsInvalid(string? email)
        => Assert.False(Make().IsValidStudentEmail(email));

    [Fact]
    public void HasSeatAvailable_True_WhenBelowLimit()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.GetSeatLimit(1)).Returns(30);
        repo.Setup(r => r.GetConfirmedCount(1)).Returns(29);

        Assert.True(Make(repo).HasSeatAvailable(1));
    }

    [Fact]
    public void HasSeatAvailable_False_WhenFull()
    {
        var repo = new Mock<IEventRepository>();
        repo.Setup(r => r.GetSeatLimit(2)).Returns(30);
        repo.Setup(r => r.GetConfirmedCount(2)).Returns(30);

        Assert.False(Make(repo).HasSeatAvailable(2));
        repo.Verify(r => r.GetSeatLimit(2), Times.Once);
    }
}

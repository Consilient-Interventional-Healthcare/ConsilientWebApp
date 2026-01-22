namespace Consilient.Users.Contracts;

public record CurrentUserDto(
    string Id,
    string UserName,
    string Email,
    IEnumerable<ClaimDto> AdditionalClaims);

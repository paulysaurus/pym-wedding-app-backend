namespace WeddingApi.DTOs;

public record RsvpSubmitDto(
    string InvitationCode,
    bool IsAttending,
    int ConfirmedPasses,
    string? DietaryRestrictions,
    string? SongRequest,
    string? MessageForCouple
);

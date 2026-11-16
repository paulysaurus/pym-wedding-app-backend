using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using WeddingApi.DTOs;
using WeddingApi.Models;

namespace WeddingApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RsvpController : ControllerBase
{
    private readonly IMongoCollection<GuestRsvp> _guests;

    public RsvpController(IMongoCollection<GuestRsvp> guests)
    {
        _guests = guests;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GuestRsvp>>> GetAll()
    {
        var guests = await _guests.Find(Builders<GuestRsvp>.Filter.Empty).ToListAsync();
        return Ok(guests);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<GuestRsvp>> GetInvitation(string code)
    {
        var filter = Builders<GuestRsvp>.Filter.Regex(
            g => g.InvitationCode,
            new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(code.Trim())}$", "i"));
        var guest = await _guests.Find(filter).FirstOrDefaultAsync();

        if (guest == null)
            return NotFound(new { message = "Código de invitación no válido." });

        return Ok(guest);
    }

    [HttpPost("confirm")]
    public async Task<ActionResult> ConfirmRsvp([FromBody] RsvpSubmitDto dto)
    {
        var filter = Builders<GuestRsvp>.Filter.Regex(
            g => g.InvitationCode,
            new MongoDB.Bson.BsonRegularExpression($"^{System.Text.RegularExpressions.Regex.Escape(dto.InvitationCode.Trim())}$", "i"));
        var guest = await _guests.Find(filter).FirstOrDefaultAsync();

        if (guest == null)
            return NotFound(new { message = "Invitación no encontrada." });

        if (dto.IsAttending && (dto.ConfirmedPasses < 1 || dto.ConfirmedPasses > guest.MaxPasses))
            return BadRequest(new { message = $"Puedes confirmar entre 1 y {guest.MaxPasses} pases." });

        guest.IsAttending = dto.IsAttending;
        guest.ConfirmedPasses = dto.IsAttending ? dto.ConfirmedPasses : 0;
        guest.DietaryRestrictions = dto.DietaryRestrictions;
        guest.SongRequest = dto.SongRequest;
        guest.MessageForCouple = dto.MessageForCouple;
        guest.RespondedAt = DateTime.UtcNow;

        await _guests.ReplaceOneAsync(g => g.Id == guest.Id, guest);

        return Ok(new { message = "Confirmación guardada con éxito.", guest });
    }
}

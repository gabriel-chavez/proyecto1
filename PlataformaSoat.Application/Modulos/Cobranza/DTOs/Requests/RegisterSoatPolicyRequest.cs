using System;

namespace PlataformaSoat.Application.Modulos.Cobranza.DTOs.Requests;

public class RegisterSoatPolicyRequest
{
    public int ClientId { get; set; }
    public string VehicleId { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public decimal Premium { get; set; }
}

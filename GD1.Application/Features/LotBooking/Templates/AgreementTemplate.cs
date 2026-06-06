using System;

namespace GD1.Application.Features.LotBooking.Templates
{
    public static class AgreementTemplate
    {
        public static string Generate(
            string customerName,
            string customerEmail,
            string vehicleBrand,
            string vehicleModel,
            string vehicleYear,
            string registrationNo,
            string vehicleType,
            string propertyName,
            string propertyAddress,
            string propertyCity,
            string propertyState,
            string startDate,
            string endDate,
            decimal pricePerDay,
            string agreementDate)
        {
            int days = Math.Max(1, (DateTime.Parse(endDate) - DateTime.Parse(startDate)).Days);
            decimal totalCost = pricePerDay * days;

            return $@"<!DOCTYPE html>
<html lang=""en"">
<head>
<meta charset=""UTF-8""/>
<style>
  @import url('https://fonts.googleapis.com/css2?family=Inter:wght@400;600;700&display=swap');
  * {{ box-sizing: border-box; margin: 0; padding: 0; }}
  body {{ font-family: 'Inter', Arial, sans-serif; color: #1a1a2e; background: #fff; font-size: 14px; line-height: 1.7; }}
  .page {{ max-width: 800px; margin: 0 auto; padding: 48px 56px; }}
  .header {{ text-align: center; border-bottom: 2px solid #000; padding-bottom: 24px; margin-bottom: 20px; }}
  .logo {{ font-size: 28px; font-weight: 700; color: #000; letter-spacing: -1px; }}
  .logo span {{ color: #000; }}
  .subtitle {{ font-size: 13px; color: #6b7280; margin-top: 4px; }}
  h1 {{ font-size: 20px; font-weight: 700; text-align: center; color: #1a1a2e; margin-bottom: 8px; }}
  .doc-id {{ text-align: center; font-size: 12px; color: #9ca3af; margin-bottom: 32px; }}
  .section {{ margin-bottom: 28px; }}
  .section-title {{ font-size: 13px; font-weight: 700; color: ##000; text-transform: uppercase; letter-spacing: 0.5px; border-left: 3px solid #000; padding-left: 10px; margin-bottom: 14px; }}
  .info-grid {{ display: grid; grid-template-columns: 1fr 1fr; gap: 10px; }}
  .info-item {{ background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 6px; padding: 10px 14px; }}
  .info-label {{ font-size: 11px; color: #9ca3af; text-transform: uppercase; letter-spacing: 0.4px; margin-bottom: 2px; }}
  .info-value {{ font-size: 13px; font-weight: 600; color: #111827; }}  
  .terms-box {{ background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px; }}
  .terms-box ol {{ padding-left: 20px; }}
  .terms-box ol li {{ margin-bottom: 10px; color: #374151; }}
  .terms-box ol li strong {{ color: #111827; }}
  .disclaimer-box {{ background: #fff7ed; border: 2px solid #fb923c; border-radius: 8px; padding: 14px 18px; margin: 16px 0 8px 0; }}
  .disclaimer-box .disc-title {{ font-weight: 700; color: #c2410c; font-size: 13px; margin-bottom: 6px; }}
  .disclaimer-box p {{ font-size: 12.5px; color: #7c2d12; line-height: 1.6; }}
  .highlight-box {{ background: #eff6ff; border: 1px solid #bfdbfe; border-radius: 8px; padding: 16px 20px; margin-bottom: 28px; }}
  .highlight-box .total {{ font-size: 18px; font-weight: 700; color: #1d4ed8; }}
  .highlight-box .total-label {{ font-size: 12px; color: #3b82f6; }}
  .footer {{ margin-top: 40px; padding-top: 20px; border-top: 1px solid #e5e7eb; display: grid; grid-template-columns: 1fr 1fr; gap: 40px; }}
  .sig-block .sig-label {{ font-size: 11px; color: #9ca3af; margin-bottom: 24px; }}
  .sig-block .sig-name {{ font-weight: 600; border-top: 1px solid #374151; padding-top: 6px; font-size: 13px; }}
  .accepted-badge {{ background: #d1fae5; border: 1px solid #6ee7b7; color: #065f46; border-radius: 6px; padding: 10px 16px; font-weight: 600; font-size: 13px; display: inline-block; }}
  .watermark {{ text-align: center; font-size: 11px; color: #d1d5db; margin-top: 24px; }}
</style>
</head>
<body>
<div class=""page"">
  <div class=""header"">
    <div class=""logo"">Grand Auto Depot One</div>
    <div class=""subtitle"">Vehicle Storage Services — Official Agreement</div>
  </div>

  <h1>Vehicle Storage Agreement</h1>
  <div class=""doc-id"">Agreement Date: {agreementDate} &nbsp;|&nbsp; Reference: GD1-AGR-{registrationNo.Replace(" ", "")}</div>

  <div class=""section"">
    <div class=""section-title"">Customer Details</div>
    <div class=""info-grid"">
      <div class=""info-item""><div class=""info-label"">Full Name</div><div class=""info-value"">{customerName}</div></div>
      <div class=""info-item""><div class=""info-label"">Email Address</div><div class=""info-value"">{customerEmail}</div></div>
    </div>
  </div>

  <div class=""section"">
    <div class=""section-title"">Vehicle Details</div>
    <div class=""info-grid"">
      <div class=""info-item""><div class=""info-label"">Brand &amp; Model</div><div class=""info-value"">{vehicleBrand} {vehicleModel} ({vehicleYear})</div></div>
      <div class=""info-item""><div class=""info-label"">Registration Number</div><div class=""info-value"">{registrationNo}</div></div>
      <div class=""info-item""><div class=""info-label"">Vehicle Type</div><div class=""info-value"">{vehicleType}</div></div>
    </div>
  </div>

  <div class=""section"">
    <div class=""section-title"">Storage Period</div>
    <div class=""info-grid"">
      <div class=""info-item""><div class=""info-label"">Property Name</div><div class=""info-value"">{propertyName}</div></div>
      <div class=""info-item""><div class=""info-label"">Location</div><div class=""info-value"">{propertyAddress}, {propertyCity}, {propertyState}</div></div>
      <div class=""info-item""><div class=""info-label"">Check-In Date</div><div class=""info-value"">{startDate}</div></div>
      <div class=""info-item""><div class=""info-label"">Check-Out Date</div><div class=""info-value"">{endDate}</div></div>
      <div class=""info-item""><div class=""info-label"">Duration</div><div class=""info-value"">{days} day(s)</div></div>
    </div>
  </div>

  <div class=""highlight-box"">
    <div class=""total-label"">Estimated Total Cost</div>
    <div class=""total"">₹{totalCost:N2}</div>
    <div style=""font-size:12px;color:#6b7280;margin-top:4px"">₹{pricePerDay:N2} × {days} day(s) = ₹{totalCost:N2}</div>
  </div>

  <div class=""section"">
    <div class=""section-title"">Terms &amp; Conditions</div>
    <div class=""terms-box"">
      <ol>
        <li><strong>Storage Responsibility:</strong> GD1 Grand Auto Depot and its partner lot owners will exercise reasonable care in storing the vehicle. However, GD1 shall not be liable for any pre-existing damage, mechanical failures, or theft arising from circumstances beyond reasonable control.</li>
        <li><strong>Vehicle Condition:</strong> The vehicle must be delivered in a roadworthy condition. Any hazardous leaks, illegal modifications, or undisclosed damage must be reported at the time of check-in.</li>
        <li><strong>Engine &amp; Mechanical Issues — Lot Owner Disclaimer:</strong> The lot owner and GD1 Grand Auto Depot are <strong>not responsible</strong> for any engine-related problems, mechanical failures, electrical faults, fluid leaks, or any internal mechanical deterioration that may occur during the storage period. Such issues are considered the sole responsibility of the vehicle owner. The storage facility provides physical space only and does not include any mechanical servicing or maintenance obligations.</li>
        <li><strong>Access &amp; Security:</strong> Access to the storage property is restricted to authorized GD1 personnel and the registered vehicle owner. No third-party access is permitted without prior written approval via the GD1 platform.</li>
        <li><strong>Duration &amp; Extension:</strong> Storage is reserved for the agreed period above. Extensions must be requested through the GD1 platform at least 24 hours before the check-out date.</li>
        <li><strong>Payment:</strong> The estimated total cost is calculated at the time of booking. Final billing may vary if the duration is extended. All payments must be settled through the GD1 platform.</li>
        <li><strong>Pickup Service:</strong> If a vehicle pickup is requested, the customer must provide a valid address. GD1 managers will be dispatched within the agreed timeframe. Pickup OTP verification is mandatory.</li>
        <li><strong>Cancellation Policy:</strong> Cancellations made more than 48 hours before the start date are eligible for a full refund. Cancellations within 48 hours may incur a one-day storage fee.</li>
        <li><strong>Damage &amp; Insurance:</strong> The vehicle owner is responsible for maintaining appropriate vehicle insurance. GD1 is not liable for any damage unless caused by proven negligence of GD1 staff.</li>
        <li><strong>Digital Acceptance:</strong> By accepting this agreement via the GD1 platform, the customer confirms they have read, understood, and agreed to all terms herein. This digital acceptance carries the same legal weight as a physical signature.</li>
        <li><strong>Governing Law:</strong> This agreement is governed by the laws of India. Any disputes shall be resolved through the appropriate courts in the jurisdiction of the storage property.</li>
      </ol>

      <div class=""disclaimer-box"">
        <div class=""disc-title"">⚠ Important Mechanical Disclaimer</div>
        <p>The lot owner and GD1 Grand Auto Depot bear <strong>no responsibility whatsoever</strong> for any engine-related issues, mechanical breakdowns, fluid degradation, or any other internal mechanical or electrical problems that occur during or after the vehicle storage period. By accepting this agreement, the vehicle owner acknowledges that the storage service covers physical space only and that the lot owner has no obligation to inspect, maintain, service, or repair the vehicle's mechanical or electrical systems at any time.</p>
      </div>
    </div>
  </div>

  <div class=""footer"">
    <div class=""sig-block"">
      <div class=""sig-label"">Customer Acceptance</div>
      <div class=""accepted-badge"">✓ Digitally Accepted</div>
      <div style=""margin-top:10px""><div class=""sig-name"">{customerName}</div></div>
      <div style=""font-size:11px;color:#9ca3af;margin-top:2px"">{agreementDate}</div>
    </div>
    <div class=""sig-block"">
      <div class=""sig-label"">Grand Auto Depot One</div>
 <div class=""accepted-badge"">✓ Digitally Accepted</div>
       <div style=""margin-top:10px""><div class=""sig-name"">Authorized Representative</div></div>
      <div style=""font-size:11px;color:#9ca3af;margin-top:2px"">GD1 Operations Team</div>
    </div>
  </div>

  <div class=""watermark"">This is a system-generated agreement from the GD1 Grand Auto Depot platform. Ref: GD1-AGR-{registrationNo.Replace(" ", "")}</div>
</div>
</body>
</html>";
        }
    }
}

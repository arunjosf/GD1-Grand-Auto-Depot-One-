const crypto = require('crypto');
const fs = require('fs');
const path = require('path');

const cloudName = "djpuczbnc";
const apiKey = "666939696269255";
const apiSecret = "TSWCzVlBenI_NWKGOWhMRnyunp8";

async function uploadImage(filePath) {
    const timestamp = Math.floor(Date.now() / 1000).toString();
    const publicId = `GD1_Auto_Depot/seed/${path.basename(filePath, '.png')}_${timestamp}`;
    
    const paramsToSign = `public_id=${publicId}&timestamp=${timestamp}`;
    const signatureRaw = paramsToSign + apiSecret;
    const signature = crypto.createHash('sha256').update(signatureRaw).digest('hex');
    
    const formData = new FormData();
    const fileBlob = new Blob([fs.readFileSync(filePath)], { type: 'image/png' });
    formData.append('file', fileBlob, path.basename(filePath));
    formData.append('api_key', apiKey);
    formData.append('timestamp', timestamp);
    formData.append('public_id', publicId);
    formData.append('signature', signature);
    
    const url = `https://api.cloudinary.com/v1_1/${cloudName}/image/upload`;
    
    try {
        const response = await fetch(url, { method: 'POST', body: formData });
        const body = await response.json();
        return body.secure_url;
    } catch (e) {
        console.error("Fetch failed", e);
        return null;
    }
}

async function run() {
    const frontImgPath = "C:\\Users\\HP\\.gemini\\antigravity\\brain\\eec48930-abe7-497f-9be3-9a5118710cff\\modern_garage_front_1780139341116.png";
    const slotPaths = [
        "C:\\Users\\HP\\.gemini\\antigravity\\brain\\eec48930-abe7-497f-9be3-9a5118710cff\\slot_1_1780139362282.png",
        "C:\\Users\\HP\\.gemini\\antigravity\\brain\\eec48930-abe7-497f-9be3-9a5118710cff\\slot_2_1780139377811.png",
        "C:\\Users\\HP\\.gemini\\antigravity\\brain\\eec48930-abe7-497f-9be3-9a5118710cff\\slot_3_1780139402332.png",
        "C:\\Users\\HP\\.gemini\\antigravity\\brain\\eec48930-abe7-497f-9be3-9a5118710cff\\slot_4_1780139430678.png"
    ];

    console.log("Uploading Front Image...");
    const frontUrl = await uploadImage(frontImgPath);
    console.log("Front URL:", frontUrl);

    const slotUrls = [];
    for (let i = 0; i < slotPaths.length; i++) {
        console.log(`Uploading Slot ${i + 1}...`);
        slotUrls.push(await uploadImage(slotPaths[i]));
    }
    console.log("Slot URLs:", slotUrls);

    const locations = [
        { name: "Neospace Parking Bay", address: "02 Neospace, KINFRA TECHNO INDUSTRIAL PARK", city: "Kakkanchery", district: "Malappuram", state: "Kerala", pin: "673635", lat: 11.1578, lon: 75.8858 },
        { name: "Hilite City Auto Vault", address: "Hilite City, Thondayad Bypass", city: "Kozhikode", district: "Kozhikode", state: "Kerala", pin: "673014", lat: 11.2588, lon: 75.8200 },
        { name: "Focus Mall Underground", address: "Focus Mall Area, Rajaji Road", city: "Kozhikode", district: "Kozhikode", state: "Kerala", pin: "673004", lat: 11.2560, lon: 75.7830 },
        { name: "Kottooli Premium Storage", address: "Kottooli", city: "Kozhikode", district: "Kozhikode", state: "Kerala", pin: "673016", lat: 11.2670, lon: 75.8110 },
        { name: "Meenchanda Transit Bay", address: "Meenchanda Bypass Road", city: "Kozhikode", district: "Kozhikode", state: "Kerala", pin: "673018", lat: 11.2223, lon: 75.8016 },
        { name: "Ramanattukara Secure Hub", address: "Ramanattukara Junction", city: "Kozhikode", district: "Kozhikode", state: "Kerala", pin: "673633", lat: 11.1764, lon: 75.8647 },
        { name: "Kondotty Airport Parking", address: "Kondotty Town, Near Airport", city: "Malappuram", district: "Malappuram", state: "Kerala", pin: "673638", lat: 11.1396, lon: 75.9620 },
        { name: "University Campus Garage", address: "Tenhipalam, University Campus", city: "Malappuram", district: "Malappuram", state: "Kerala", pin: "673636", lat: 11.1309, lon: 75.8961 },
        { name: "Down Hill Auto Safe", address: "Down Hill", city: "Malappuram", district: "Malappuram", state: "Kerala", pin: "676505", lat: 11.0423, lon: 76.0716 },
        { name: "Manjeri Central Parking", address: "Manjeri Town", city: "Malappuram", district: "Malappuram", state: "Kerala", pin: "676121", lat: 11.1186, lon: 76.1219 }
    ];

    let sql = `SET NOCOUNT ON;\nDECLARE @PropId BIGINT;\n`;
    for (let i = 0; i < locations.length; i++) {
        let loc = locations[i];
        sql += `
INSERT INTO VehicleStorageProperties (LotOwnerId, LotCode, Name, Description, AddressLine, City, State, Country, Latitude, Longitude, Status, HasCCTV, HasSecurity, HasFireSafety, HasWorkshopBay, HasWashingArea, ExtraFacilities, PricePerDay, AverageRating, TotalReviews, CreatedAt, UpdatedAt, IsDeleted)
VALUES (1, 'GD1-KE-${String(i+100).padStart(4, '0')}', '${loc.name}', 'Premium secure vehicle storage located in ${loc.city}. Fully certified.', '${loc.address}', '${loc.city}', '${loc.state}', 'India', ${loc.lat}, ${loc.lon}, 'Active', 1, 1, 1, 1, 1, 'EV Charging,Valet,Washing', 500.00 + ${Math.floor(Math.random() * 300)}, 4.5 + ${Math.random() * 0.5}, ${Math.floor(Math.random() * 50) + 5}, GETUTCDATE(), GETUTCDATE(), 0);
SET @PropId = SCOPE_IDENTITY();
`;
        // Insert front image
        sql += `INSERT INTO PropertyImages (ApplicationId, VehicleStoragePropertyId, ImageUrl, Label, UploadedBy, IsMain, CreatedAt, UpdatedAt, IsDeleted) VALUES (NULL, @PropId, '${frontUrl}', 'Property Main', 'System', 1, GETUTCDATE(), GETUTCDATE(), 0);\n`;
        
        // Insert 4 slots
        for (let j = 0; j < 4; j++) {
            sql += `
INSERT INTO VehicleStorageSlots (PropertyId, SlotNumber, SlotType, IsOccupied, SquareFeet, HeightFeet, ImageUrl, CreatedAt, UpdatedAt, IsDeleted)
VALUES (@PropId, 'A-${101 + j}', 'Private Garage', 0, 200.0, 12.0, '${slotUrls[j]}', GETUTCDATE(), GETUTCDATE(), 0);
`;
        }
    }
    
    fs.writeFileSync('seed.sql', sql);
    console.log("SQL script generated to seed.sql");
}

run();

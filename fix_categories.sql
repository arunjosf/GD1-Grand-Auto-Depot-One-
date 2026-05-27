-- ============================================================
-- STEP 1: Mark all motorcycle models correctly
-- BMW motorcycles: F-series, R-series, K-series, C-series, G-series, S-series, HP, M 1000
-- Honda: CB, CBR, CRF, CMX, CTX, GL, NC, NM4, PCX, SH, VFR, VT, XL, XRV, ADV, Africa Twin, etc.
-- Suzuki: GS, GSX, GSF, DR, RG, SV, Boulevard, TL, V-Strom, Hayabusa, Intruder, etc.
-- ============================================================
UPDATE VehicleCatalog SET Category = 'Motorcycle', LengthFeet = 7.0, WidthFeet = 3.0, HeightFeet = 4.2
WHERE Brand = 'BMW' AND (
    Model LIKE 'C %' OR Model LIKE 'C4%' OR Model LIKE 'CE %'
    OR Model LIKE 'F %' OR Model LIKE 'G %'
    OR Model LIKE 'HP%'
    OR Model LIKE 'K %' OR Model LIKE 'K1%' OR Model LIKE 'K7%' OR Model LIKE 'K100%'
    OR Model LIKE 'R %' OR Model LIKE 'R1%' OR Model LIKE 'R6%' OR Model LIKE 'R8%' OR Model LIKE 'R9%'
    OR Model LIKE 'S 1000%'
    OR Model LIKE 'M 1000%'
    OR Model LIKE 'CE%'
    OR Model IN ('R18', 'R nineT')
    OR Model LIKE 'R nineT%'
    OR Model LIKE 'R 18%'
);

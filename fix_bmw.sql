-- BMW SUV (X-series) fixes
UPDATE VehicleCatalog SET Category = 'SUV', LengthFeet = 15.5, WidthFeet = 6.2, HeightFeet = 5.8
WHERE Brand = 'BMW' AND (Model LIKE 'X1%' OR Model LIKE 'X2%' OR Model LIKE 'X3%' OR Model LIKE 'X4%'
    OR Model LIKE 'X5%' OR Model LIKE 'X6%' OR Model LIKE 'X7%' OR Model = 'XM');

-- BMW Coupe/Convertible fixes
UPDATE VehicleCatalog SET Category = 'Coupe', LengthFeet = 14.5, WidthFeet = 6.0, HeightFeet = 4.7
WHERE Brand = 'BMW' AND (Model LIKE '%Ci' OR Model LIKE '%iS' OR Model LIKE '%CSi' OR Model LIKE '%CSI'
    OR Model IN ('840i','850i','840Ci','850Ci','850CSi','630Ci','635CSi','633 csi','645Ci','840i','M4','M8','M850i')
    OR Model LIKE '?28' OR Model LIKE '?30%' OR Model LIKE '?35%' OR Model LIKE '?40%' OR Model LIKE '?28i'
    OR Model IN ('228','228i','230i','428i','430i','435i','440i','640i','640xi','645i','650i','650xi','M2','M235','M235i','M240i','M440i','M6','M660i','M8')
    OR Model LIKE '2 Series%' OR Model LIKE '4 Series%' OR Model LIKE '6 Series%' OR Model LIKE '8 Series%');

-- BMW Sedan fixes (3, 5, 7 series sedans)
UPDATE VehicleCatalog SET Category = 'Sedan', LengthFeet = 15.5, WidthFeet = 6.0, HeightFeet = 4.8
WHERE Brand = 'BMW' AND (
    Model LIKE '3%' OR Model LIKE '5%' OR Model LIKE '7%' OR Model LIKE 'M3%' OR Model LIKE 'M5%' OR Model LIKE 'M340%' OR Model LIKE 'M550%' OR Model LIKE 'M760%' OR Model LIKE 'M7%'
    OR Model IN ('i3','i4','i5','i7','ActiveHybrid 3','ActiveHybrid 5','ActiveHybrid 7','ActiveE')
) AND Category NOT IN ('SUV','Motorcycle','Coupe');

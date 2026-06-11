import re

path = r'c:\Users\HP\source\repos\GD1(Grand Auto Depot One)\GD1.Infrastructure\Repositories\BookingReadRepository.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

old_join = r'''LEFT JOIN PickupVerifications pv_pickup ON pv_pickup.BookingId = b.Id AND pv_pickup.Type = 0
                LEFT JOIN PickupVerifications pv_arrival ON pv_arrival.BookingId = b.Id AND pv_arrival.Type = 1'''

new_join = '''OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 0 ORDER BY pv.Id DESC) pv_pickup
                OUTER APPLY (SELECT TOP 1 * FROM PickupVerifications pv WHERE pv.BookingId = b.Id AND pv.Type = 1 ORDER BY pv.Id DESC) pv_arrival'''

content = content.replace(old_join, new_join)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)

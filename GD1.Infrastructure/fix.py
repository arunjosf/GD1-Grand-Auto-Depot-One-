import re

path = r'c:\Users\HP\source\repos\GD1(Grand Auto Depot One)\GD1.Infrastructure\Repositories\BookingReadRepository.cs'
with open(path, 'r', encoding='utf-8') as f:
    content = f.read()

old_case = r'''CASE pr.Status
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus'''

new_case = '''CASE pr.Status
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
                       END AS PickupStatus'''

# Some might have 'WHEN 0 THEN 'Requested'' too.
old_case2 = r'''CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'OtpSent'
                           WHEN 5 THEN 'OwnerOtpSubmitted'
                           WHEN 6 THEN 'Verified'
                           WHEN 7 THEN 'VehiclePicked'
                           WHEN 8 THEN 'InTransit'
                           WHEN 9 THEN 'Stored'
                       END AS PickupStatus'''

new_case2 = '''CASE pr.Status
                           WHEN 0 THEN 'Requested'
                           WHEN 1 THEN 'Assigned'
                           WHEN 2 THEN 'ManagerScheduled'
                           WHEN 3 THEN 'Approved'
                           WHEN 4 THEN 'Declined'
                           WHEN 5 THEN 'OtpSent'
                           WHEN 6 THEN 'OwnerOtpSubmitted'
                           WHEN 7 THEN 'Verified'
                           WHEN 8 THEN 'VehiclePicked'
                           WHEN 9 THEN 'InTransit'
                           WHEN 10 THEN 'Stored'
                       END AS PickupStatus'''

content = re.sub(r'CASE pr\.Status\s+WHEN 1 THEN \'Assigned\'.*?END AS PickupStatus', new_case, content, flags=re.DOTALL)
content = re.sub(r'CASE pr\.Status\s+WHEN 0 THEN \'Requested\'.*?END AS PickupStatus', new_case2, content, flags=re.DOTALL)

with open(path, 'w', encoding='utf-8') as f:
    f.write(content)


# Tenant Entities

## Tenant
* UId: PK
* TenantId: Ext
* ParentUId: FK null
* Name: string 
* Description: string null

## Location
* UId: PK
* TenantUId: FK
* LocationId: Ext
* Name: string 
* Description: string null




# Production Entities

## Machinetool
* UId: PK
* TenantUId: FK 
* MachineId: Ext
* MachinetoolTypeUId: FK null
* MachinetoolStateCode: string null
* LocationUId: FK null 
* Name: string null
* Description: string null
* Vendor: string null
* AssetUId: FK null
* BuildDateUtc: datetimeoffset null
* InstallationDateUtc: datetimeoffset null
* DismissionDateUtc: datetimeoffset null

## MachinetoolType
* UId: PK
* TenantUId: FK 
* MachinetoolTypeId: Ext
* Name: string
* Description: string null

## MachinetoolGroup
* UId: PK
* TenantUId: FK
* MachinetoolGroupId: Ext
* Name: string 
* Description: string null

## MachinetoolGroupRelation
* MachinetoolUId: FK
* MachinetoolGroupUId: FK

## MachinetoolState
* MachinetoolStateCode: string 
* TenantUId: FK
* Description: string null
* Note: string null



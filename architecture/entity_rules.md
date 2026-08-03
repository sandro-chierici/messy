---
name: create-entity
description: How to create an entity in the system, naming conventions, and best practices.
---

# create entity


## General Rules: 
* Any entity represent a C# Entity.
* Any name for Entities and properties is in Pascal Case into C# entities, BUT needs to be annotate with:   
  Table("<table name>")  for Entities   
  Column("<column name>") for properties  
  Any table name NEEDS to be in snake case  
  Any column name NEEDS to be in snake_case  
* PK are ALWAYS  "UId" And Int64 (bigint)
* Any Property marked with Ext is GUID V7 
* Any property that is an FK is Int64 
* Any c# entity is in a separate .cs file with the name of the entity
* Any Entity name is Singular BUT table name is plural 

## Rules for ER MD's files

* Entities start with ##  
* Primary Keys are marked with PK 
* "Ext" marks GUID V7 props
* Any other prop c# type and nullability is marked after prop name and ":" for example  MachineName: string null 
* In the entity propetries are marked with bullet points


## Example for generating entities: 

###  Tool
* UId: PK
* ToolId: Ext
* LocationUId: FK null 
* CreationDateUtc: datetimeoffset

``` C# 
/// <summary>
/// Tool Data Model
/// </summary>
[Table("tools")]
public sealed class Tool
{
    [Column("uid")]
    public Int64 UId { get; set; } 

    [Column("tool_id")]
    public required Guid ToolId { get; init; }

    [Column("location_uid")]
    public Int64? LocationUId { get; set; }    

    [Column("utc_created")]
    public DateTimeOffset CreationDateUtc { get; set; } = DateTimeOffset.UtcNow;
}

```
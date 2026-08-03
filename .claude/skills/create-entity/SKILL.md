---
name: create-entity
description: How to create an entity in the system, naming conventions, and best practices.
---

# Create Entity

## Overview
Creating an entity in the system involves defining a new C# class that can be used to store and manage specific types of data. This process includes naming the entity, defining its attributes, and setting up any necessary relationships with other entities.  
## Naming Conventions
When naming an entity, it is important to follow these conventions to ensure consistency and clarity:       
1. Use PascalCase for entity classes (e.g., `Customer`, `Order`, `Product`).  
2. Avoid using spaces or special characters in entity names.      
3. Choose descriptive names that clearly indicate the purpose of the entity.
## Best Practices
1. Define clear and concise attributes for the entity, ensuring that they are relevant to the data being stored.  
2. Establish relationships with other entities where necessary to maintain data integrity and enable efficient querying.  
3. Regularly review and update the entity definitions to accommodate changes in business requirements or data structure.
4. Document the entity and its attributes thoroughly to facilitate understanding and maintenance by other team members.
## Example
Here is an example of how to create a `Customer` entity:
```json 
{
  "name": "Customer",
  "attributes": [
    {
      "name": "CustomerID",
      "type": "String",
      "description": "A unique identifier for each customer."
    },
    {
      "name": "FirstName",
      "type": "String",
      "description": "The customer's first name."
    },
    {
      "name": "LastName",
      "type": "String",
      "description": "The customer's last name."
    },
    {
      "name": "Email",
      "type": "String",
      "description": "The customer's email address."
    }
  ],
  "relationships": [
    {
      "name": "Orders",
      "type": "OneToMany",
      "targetEntity": "Order"
    }
  ]
}
```
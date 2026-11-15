using Domain.Entities;

namespace Domain.Constants;

public static class Permissions
{
    public static class Users
    {
        public const string Read = "Users.Read";
        public const string Create = "Users.Create";
        public const string Update = "Users.Update";
        public const string Delete = "Users.Delete";
    }

    public static class Roles
    {
        public const string Read = "Roles.Read";
        public const string Create = "Roles.Create";
        public const string Update = "Roles.Update";
        public const string Delete = "Roles.Delete";
    }

    public static class Categories
    {
        public const string Read = "Categories.Read";
        public const string Create = "Categories.Create";
        public const string Update = "Categories.Update";
        public const string Delete = "Categories.Delete";
    }

    public static class Products
    {
        public const string Read = "Products.Read";
        public const string Create = "Products.Create";
        public const string Update = "Products.Update";
        public const string Delete = "Products.Delete";
    }

    public static class InventoryMovements
    {
        public const string Read = "InventoryMovements.Read";
        public const string Create = "InventoryMovements.Create";
    }

    public static IReadOnlyList<PermissionCreationData> GetAll()
    {
        return
        [
            new(Users.Read, "Read Users"),
            new(Users.Create, "Create Users"),
            new(Users.Update, "Update Users"),
            new(Users.Delete, "Delete Users"),
            new(Roles.Read, "Read Roles"),
            new(Roles.Create, "Create Roles"),
            new(Roles.Update, "Update Roles"),
            new(Roles.Delete, "Delete Roles"),
            new(Categories.Read, "Read Categories"),
            new(Categories.Create, "Create Categories"),
            new(Categories.Update, "Update Categories"),
            new(Categories.Delete, "Delete Categories"),
            new(Products.Read, "Read Products"),
            new(Products.Create, "Create Products"),
            new(Products.Update, "Update Products"),
            new(Products.Delete, "Delete Products"),
            new(InventoryMovements.Read, "Read Inventory Movements"),
            new(InventoryMovements.Create, "Create Inventory Movements")
        ];
    }
}

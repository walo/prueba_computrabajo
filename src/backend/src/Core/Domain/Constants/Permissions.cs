using Domain.Entities;

namespace Domain.Constants;

public static class Permissions
{
    public const string UsersRead = "Users.Read";
    public const string UsersCreate = "Users.Create";
    public const string UsersUpdate = "Users.Update";
    public const string UsersDelete = "Users.Delete";
    public const string RolesRead = "Roles.Read";
    public const string RolesCreate = "Roles.Create";
    public const string RolesUpdate = "Roles.Update";
    public const string RolesDelete = "Roles.Delete";
    public const string CategoriesRead = "Categories.Read";
    public const string CategoriesCreate = "Categories.Create";
    public const string CategoriesUpdate = "Categories.Update";
    public const string CategoriesDelete = "Categories.Delete";
    public const string ProductsRead = "Products.Read";
    public const string ProductsCreate = "Products.Create";
    public const string ProductsUpdate = "Products.Update";
    public const string ProductsDelete = "Products.Delete";
    public const string InventoryMovementsRead = "InventoryMovements.Read";
    public const string InventoryMovementsCreate = "InventoryMovements.Create";

    public static IReadOnlyList<PermissionCreationData> GetAll()
    {
        return
        [
            new(UsersRead, "Read Users"),
            new(UsersCreate, "Create Users"),
            new(UsersUpdate, "Update Users"),
            new(UsersDelete, "Delete Users"),
            new(RolesRead, "Read Roles"),
            new(RolesCreate, "Create Roles"),
            new(RolesUpdate, "Update Roles"),
            new(RolesDelete, "Delete Roles"),
            new(CategoriesRead, "Read Categories"),
            new(CategoriesCreate, "Create Categories"),
            new(CategoriesUpdate, "Update Categories"),
            new(CategoriesDelete, "Delete Categories"),
            new(ProductsRead, "Read Products"),
            new(ProductsCreate, "Create Products"),
            new(ProductsUpdate, "Update Products"),
            new(ProductsDelete, "Delete Products"),
            new(InventoryMovementsRead, "Read Inventory Movements"),
            new(InventoryMovementsCreate, "Create Inventory Movements")
        ];
    }
}

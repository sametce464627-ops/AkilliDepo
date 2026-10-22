namespace AkilliDepo.Models;

public sealed class Admin : User
{
    public Admin(int id, string username, string password, string fullName)
        : base(id, username, password, fullName) { }

    public override string RoleName => "Yönetici";
    public override bool CanDeleteProduct() => true;
    public override bool CanManageSuppliers() => true;
    public override bool CanManageCategories() => true;
    public override bool CanManageProducts() => true;
    public override bool CanStockOut() => true;
    public override bool CanViewReports() => true;
    public override bool CanViewMovements() => true;
}

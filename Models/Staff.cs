namespace AkilliDepo.Models;

public sealed class Staff : User
{
    public Staff(int id, string username, string password, string fullName)
        : base(id, username, password, fullName) { }

    public override string RoleName => "Personel";
    public override bool CanDeleteProduct() => false;
    public override bool CanManageSuppliers() => false;
    public override bool CanManageCategories() => false;
    public override bool CanManageProducts() => false;
    public override bool CanStockOut() => false;
    public override bool CanViewReports() => false;
    public override bool CanViewMovements() => false;
}

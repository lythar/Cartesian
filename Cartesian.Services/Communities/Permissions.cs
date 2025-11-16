namespace Cartesian.Services.Communities;

[Flags]
public enum Permissions : UInt32
{
    None = 0,
    Member = 1 << 0,
    All = 0x3FFFFFFF,
    Admin = All | 1u << 30,
    Owner = Admin | 1u << 31
}

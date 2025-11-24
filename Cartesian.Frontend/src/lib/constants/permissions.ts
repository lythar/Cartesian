export enum Permissions {
	None = 0,
	Member = 1 << 0,
	ManageEvents = 1 << 1,
	ManageWindows = 1 << 2,
	ManagePeople = 1 << 3,
	ManageCommunity = 1 << 4,
	ManageChat = 1 << 5,
	All = 0x3fffffff,
	Admin = All | (1 << 30),
	Owner = Admin | (1 << 31),
}

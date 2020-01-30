namespace DetentionRoom.Networking
{
    public enum Classes
    {
        Unassigned,
        Teacher,
        Student
    }

    public enum Stage
    {
        Dunno,
        Kitchen,
        Keys
    }

    public enum Levels
    {
        Menu,
        Game
    }

    public enum PlayerState
    {
        Idle,
        Walking,
        Crouching,
        Shooting,
        Respawning,
        Reloading
    }
    
    public enum WeaponType
    {
        Chalk,
        Stapler,
        Slingshot,
        Ruler,
        Sponge
    }
    
    public enum MedicType
    {
        Health,
    }
    
    public enum AmmoType
    {
        Chalk,
        Stapler,
        Slingshot
    }

    public enum BodyPart
    {
        Head,
        Stomach,
        Shoulder
    }

    public enum SupplyType
    {
        Weapon_Chalk,
        Weapon_Stapler,
        Weapon_Slingshot,
        Weapon_Ruler,
        Weapon_Sponge,
        Ammo_Chalk,
        Ammo_Stapler,
        Ammo_Slingshot
    }
}
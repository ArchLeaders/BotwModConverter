using Cead;

namespace BotwModConverter.Core.Converters;

public class ActorInfoConverter : Converter
{
    private readonly Dictionary<string, double> _ratios = new() {
        { "ActorOption", 1.47986 },
        { "ActorReaction", 1.95015 },
        { "AirWall", 1.55907 },
        { "Anchor", 1.54027 },
        { "Area", 1.57014 },
        { "AreaManagementForOthers", 1.56662 },
        { "AreaManagementForPlayer", 1.57471 },
        { "ArmorExtra0", 3.24801 },
        { "ArmorExtra1", 3.64020 },
        { "ArmorExtra2", 2.81442 },
        { "ArmorHead", 2.27533 },
        { "ArmorLower", 2.77174 },
        { "ArmorUpper", 2.11312 },
        { "Beam", 1.58558 },
        { "Bullet", 2.12249 },
        { "Camera", 1.53002 },
        { "CapturedActor", 2.33165 },
        { "ComplexTag", 1.58580 },
        { "CookResult", 1.47792 },
        { "DemoEquipment", 2.78588 },
        { "DemoNPC", 2.44085 },
        { "Dragon", 1.62425 },
        { "Dummy", 1.57355 },
        { "EditCamera", 1.59361 },
        { "EffectLocater", 1.68679 },
        { "Enemy", 1.70107 },
        { "EnemySwarm", 2.03859 },
        { "EnvSeEmitPoint", 1.59169 },
        { "EventSystem", 1.58667 },
        { "EventTag", 1.59889 },
        { "GameBlanceLocater", 2.71996 },
        { "GelEnemy", 1.69351 },
        { "GiantArmor", 2.74482 },
        { "GiantEnemy", 1.64212 },
        { "GlobalParameter", 1.55041 },
        { "Guardian", 2.00911 },
        { "GuardianComponent", 2.75823 },
        { "Horse", 1.57119 },
        { "HorseObject", 1.96967 },
        { "HorseReins", 2.78040 },
        { "HorseSaddle", 2.41971 },
        { "Item", 2.59118 },
        { "LastBoss", 1.68402 },
        { "LineBeam", 1.58024 },
        { "LinkTag", 1.67059 },
        { "MapConstActive", 2.62619 },
        { "MapConstPassive", 3.34795 },
        { "MapDynamicActive", 2.54314 },
        { "MapDynamicPassive", 2.87003 },
        { "MergedDungeonParts", 2.26925 },
        { "Motorcycle", 1.89353 },
        { "NoCalcActor", 1.56243 },
        { "NoWork", 1.55033 },
        { "NPC", 1.75036 },
        { "OptionalWeapon", 3.32726 },
        { "PauseMenuPlayer", 1.86665 },
        { "Player", 1.44917 },
        { "PlayerItem", 1.82444 },
        { "Prey", 1.90845 },
        { "Remains", 2.23782 },
        { "Rope", 2.45780 },
        { "Sandworm", 1.66948 },
        { "SiteBoss", 1.61253 },
        { "SoleTag", 1.60180 },
        { "SoundProxy", 1.57575 },
        { "SoundSystemActor", 1.66232 },
        { "SpotBgmTag", 1.54055 },
        { "StaticAnchor", 1.53173 },
        { "Swarm", 2.29493 },
        { "SweepCollision", 1.53318 },
        { "System", 1.59686 },
        { "Weapon", 2.47555 },
        { "WeaponBow", 2.43674 },
        { "WeaponLargeSword", 2.40451 },
        { "WeaponShield", 2.58867 },
        { "WeaponSmallSword", 2.56363 },
        { "WeaponSpear", 2.41490 },
        { "WolfLink", 1.87220 },
    };

    public override Span<byte> ConvertToSwitch(ReadOnlySpan<byte> data)
    {
        Byml byml = Byml.FromBinary(data);
        UpdateInstSize(byml, toNx: true);
        NativeHandle = byml.ToBinary(out Span<byte> converted, bigEndian: false);
        return converted;
    }

    public override Span<byte> ConvertToWiiu(ReadOnlySpan<byte> data)
    {
        Byml byml = Byml.FromBinary(data);
        UpdateInstSize(byml, toNx: false);
        NativeHandle = byml.ToBinary(out Span<byte> converted, bigEndian: true);
        return converted;
    }

    private void UpdateInstSize(Byml byml, bool toNx)
    {
        foreach (var item in byml.GetHash()["Array"].GetArray()) {
            Byml.Hash actor = item.GetHash();
            if (actor.Contains("instSize") && actor.Contains("profile")) {
                ulong instSize = actor["instSize"].GetUInt64();
                double ratio = _ratios[actor["profile"].GetString()!];
                actor["instSize"] = (ulong)(toNx ? instSize * ratio : instSize / ratio);
            }
        }
    }
}

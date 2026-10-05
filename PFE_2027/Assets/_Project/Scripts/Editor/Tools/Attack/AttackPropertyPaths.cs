using PFE.Core.Scripts.Enemy.Attacks;

namespace PFE.Editor._Project.Scripts.Editor.Tools.Attack
{
    // Noms des champs sérialisés de l'AttackData, centralisés.
    // nameof() : si une propriété est renommée, la compilation casse ici au lieu d'échouer en silence.
    internal static class AttackPropertyPaths
    {
        public const string Hitboxes = "hitboxes";

        public static readonly string MovementLock = Backing(nameof(AttackData.MovementLock));
        public static readonly string ComboWindow = Backing(nameof(AttackData.ComboWindow));
        public static readonly string Next = Backing(nameof(AttackData.Next));

        public static readonly string Window = Backing(nameof(HitboxWindow.Window));
        public static readonly string Shape = Backing(nameof(HitboxWindow.Shape));
        public static readonly string Offset = Backing(nameof(HitboxWindow.Offset));
        public static readonly string Rotation = Backing(nameof(HitboxWindow.Rotation));
        public static readonly string Size = Backing(nameof(HitboxWindow.Size));

        public static readonly string Start = Backing(nameof(TimeWindow.Start));
        public static readonly string End = Backing(nameof(TimeWindow.End));

        // Champ caché généré par le compilateur pour une propriété [field: SerializeField]
        public static string Backing(string propertyName) => $"<{propertyName}>k__BackingField";
    }
}

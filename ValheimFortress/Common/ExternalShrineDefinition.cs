using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ValheimFortress.Common
{
    public enum LevelSelectionStrategy
    {
        Random,
        ExternallySet,
        Incremental
    }

    public class ExternalShrineDefinition
    {
        public string NameLocalization { get; }
        public string ShrineName { get; }
        public bool RequiresSacrifice { get; } = false;
        public string RequestLocalization { get; }
        public LevelSelectionStrategy LevelSelection { get; } = LevelSelectionStrategy.Random;
        void Create() {

        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using DataAccess.Repository;

namespace ImportManager.Adapters
{
    public abstract class AbstractImportAdapter
    {
        //Stage 1 (Manual): Select file/source
        //Stage 2 (Auto): Pre-parsing
        //Stage 3 (Manual): Input parameters/selection
        //Stage 4 (Auto): Process input
        public enum ImportStage { InputSelection, PreParsing, ParameterSelection, Processing, Finished }
        internal ImportStage CurrentStage { get; set; }
        public string Title { get; private set; }

        public AbstractImportAdapter()
        {
            CurrentStage = ImportStage.InputSelection;
        }

        public void PerformImport()
        {
            while (CurrentStage != ImportStage.Finished)
            {
                switch (CurrentStage)
                {
                    case ImportStage.InputSelection:
                        CurrentStage = performInputSelection();
                        break;
                    case ImportStage.PreParsing:
                        CurrentStage = performPreParsing();
                        break;
                    case ImportStage.ParameterSelection:
                        CurrentStage = performParameterSelection();
                        break;
                    case ImportStage.Processing:
                        CurrentStage = performProcessing();
                        break;
                    case ImportStage.Finished:
                        break;
                }
            }
        }

        protected abstract ImportStage performInputSelection();
        protected abstract ImportStage performPreParsing();
        protected abstract ImportStage performParameterSelection();
        protected abstract ImportStage performProcessing();
    }
}

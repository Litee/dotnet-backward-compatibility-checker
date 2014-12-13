using System;
using System.IO;
using System.Linq;
using BackwardCompatibilityChecker.Introspection.Types;

namespace BackwardCompatibilityChecker.Introspection.Diff
{
    public class DiffPrinter
    {
        private readonly TextWriter _writer;

        /// <summary>
        /// Print diffs to console
        /// </summary>
        public DiffPrinter(TextWriter writer)
        {
            this._writer = writer;
        }

        internal void Print(AssemblyDiffCollection diff)
        {
            if (diff.RemovedTypes.Any())
            {
                this._writer.WriteLine("\tRemoved {0} public type/s", diff.RemovedTypes.Count);
                foreach (var remType in diff.RemovedTypes)
                {
                    this._writer.WriteLine("\t\t- {0}", remType.Print());
                }
            }

            if (diff.AddedTypes.Any())
            {
                this._writer.WriteLine("\tAdded {0} public type/s", diff.AddedTypes.Count);
                foreach (var addedType in diff.AddedTypes)
                {
                    this._writer.WriteLine("\t\t+ {0}", addedType.Print());
                }
            }

            if (diff.ChangedTypes.Count > 0)
            {
                foreach (var typeChange in diff.ChangedTypes)
                {
                    this.PrintTypeChanges(typeChange);
                }
            }
        }

        private void PrintTypeChanges(TypeDiff typeChange)
        {
            this._writer.WriteLine("\t" + typeChange.TypeV1.Print());
            if (typeChange.HasChangedBaseType)
            {
                this._writer.WriteLine("\t\tBase type changed: {0} -> {1}",
                    typeChange.TypeV1.IsNotNull( () =>
                        typeChange.TypeV1.BaseType.IsNotNull( () => typeChange.TypeV1.BaseType.FullName)),
                    typeChange.TypeV2.IsNotNull(()=>
                        typeChange.TypeV2.BaseType.IsNotNull( () => typeChange.TypeV2.BaseType.FullName))
                );
            }

            if (typeChange.Interfaces.Count > 0)
            {
                foreach (var addedItf in typeChange.Interfaces.Added)
                {
                    this._writer.WriteLine("\t\t+ interface: {0}", addedItf.ObjectV1.FullName);
                }
                foreach (var removedItd in typeChange.Interfaces.Removed)
                {
                    this._writer.WriteLine("\t\t- interface: {0}", removedItd.ObjectV1.FullName);
                }
            }

            foreach(var addedEvent in typeChange.Events.Added)
            {
                this._writer.WriteLine("\t\t+ {0}", addedEvent.ObjectV1.Print());
            }

            foreach(var remEvent in typeChange.Events.Removed)
            {
                this._writer.WriteLine("\t\t- {0}", remEvent.ObjectV1.Print());
            }

            foreach(var addedField in typeChange.Fields.Added)
            {
                this._writer.WriteLine("\t\t+ {0}", addedField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach(var remField in typeChange.Fields.Removed)
            {
                this._writer.WriteLine("\t\t- {0}", remField.ObjectV1.Print(FieldPrintOptions.All));
            }

            foreach(var addedMethod in typeChange.Methods.Added)
            {
                this._writer.WriteLine("\t\t+ {0}", addedMethod.ObjectV1.Print(MethodPrintOption.Full));
            }

            foreach(var remMethod in typeChange.Methods.Removed)
            {
                this._writer.WriteLine("\t\t- {0}", remMethod.ObjectV1.Print(MethodPrintOption.Full));
            }
        }
    }
}

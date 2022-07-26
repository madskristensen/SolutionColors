//using System.Collections.Generic;
//using System.ComponentModel.Composition;
//using Microsoft.VisualStudio;
//using Microsoft.VisualStudio.OLE.Interop;
//using Microsoft.VisualStudio.Workspace.VSIntegration.UI;

//namespace SolutionColors.Commands
//{
//    [Export(typeof(INodeExtender))]
//    public class SetColorCommandFolderProvider : INodeExtender
//    {
//        private readonly IWorkspaceCommandHandler _handler = new SetColorCommandFolder();

//        public IChildrenSource ProvideChildren(WorkspaceVisualNodeBase parentNode) => null;

//        public IWorkspaceCommandHandler ProvideCommandHandler(WorkspaceVisualNodeBase parentNode)
//        {
//            if (parentNode is IFolderNode)
//            {
//                return _handler;
//            }

//            return null;
//        }
//    }

//    public class SetColorCommandFolder : IWorkspaceCommandHandler
//    {
//        public bool IgnoreOnMultiselect => true;

//        public int Priority => 100;

//        public int Exec(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
//        {
//            ThreadHelper.ThrowIfNotOnUIThread();

//            if (IsSupportedCommand(pguidCmdGroup, nCmdID))
//            {
//                VS.MessageBox.Show(selection[0].Text);

//                    return VSConstants.S_OK;
//            }

//            return (int)Constants.OLECMDERR_E_NOTSUPPORTED;
//        }

//        public bool QueryStatus(List<WorkspaceVisualNodeBase> selection, Guid pguidCmdGroup, uint nCmdID, ref uint cmdf, ref string customTitle)
//        {
//            if (IsSupportedCommand(pguidCmdGroup, nCmdID))
//            {
//                if (selection[0].Root != selection[0])
//                {
//                    cmdf = (uint)(OLECMDF.OLECMDF_INVISIBLE);
//                    return true;
//                }
//                else
//                {
//                    cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
//                    return true;
//                }
//            }

//            return false;
//        }

//        private static bool IsSupportedCommand(Guid pguidCmdGroup, uint nCmdID)
//        {
//            return pguidCmdGroup == PackageGuids.SolutionColors;
//        }
//    }
//}

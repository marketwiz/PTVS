﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.PythonTools.Cdp;
using Microsoft.PythonTools.Interpreter;
using Microsoft.PythonTools.Parsing;

namespace Microsoft.PythonTools.Analysis.Communication {
    public static class AnalysisProtocol {
        public static readonly Dictionary<string, Type> RegisteredTypes = CollectCommands();

        private static Dictionary<string, Type> CollectCommands() {
            Dictionary<string, Type> all = new Dictionary<string, Type>();
            foreach (var type in typeof(AnalysisProtocol).GetNestedTypes()) {
                if (type.IsSubclassOf(typeof(Request))) {
                    var command = type.GetField("Command");
                    if (command != null) {
                        all["request." + (string)command.GetRawConstantValue()] = type;
                    }
                } else if (type.IsSubclassOf(typeof(Event))) {
                    var name = type.GetField("Name");
                    if (name != null) {
                        all["event." + (string)name.GetRawConstantValue()] = type;
                    }
                }
            }
            return all;
        }


        public class QuickInfoRequest : Request<QuickInfoResponse> {
            public const string Command = "quickInfo";

            public int fileId;
            public string expr;
            public int line, column, index;

            public override string command => Command;
        }

        public class QuickInfoResponse : Response {
            public string text;
        }

        public class FileParsedEvent : Event {
            public const string Name = "fileParsed";

            public int fileId, version;
            public bool hasErrors;
            public Error[] errors;
            public Error[] warnings;
            public TaskItem[] tasks;

            public override string name => Name;
        }

        public struct CodeSpan {
            public int start, length;
        }

        public class Error {
            public string message;
            public int startLine, startColumn, startIndex;
            public int endLine, endColumn, length;
        }

        public class AddFileRequest : Request<AddFileResponse> {
            public const string Command = "addFile";

            public string path;
            public string addingFromDir;

            public override string command => Command;
        }

        public class AddFileResponse : Response {
            public int fileId;
        }

        public sealed class AddDirectoryRequest : Request<Response> {
            public const string Command = "addDir";

            public string dir;
            public override string command => Command;
        }

        public sealed class RemoveDirectoryRequest : Request<Response> {
            public const string Command = "removeDir";

            public string dir;

            public override string command => Command;
        }

        public sealed class UnloadFileRequest : Request<Response> {
            public const string Command = "unloadFile";

            public int fileId;
            public override string command => Command;
        }


        public sealed class DirectoryFileAddedEvent : Event {
            public const string Name = "directoryFileAdded";

            public string filename;
            public int fileId;

            public override string name => Name;
        }

        public sealed class FileContentRequest : Request<Response> {
            public const string Command = "fileContent";

            public override string command => Command;

            public int fileId, version;
            public string content;
        }

        public sealed class FileChangedRequest : Request<FileChangedResponse> {
            public const string Command = "fileChanged";

            public override string command => Command;

            public int fileId, version;
            public ChangeInfo[] changes;
        }

        public sealed class UnresolvedImportsRequest  : Request<UnresolvedImportsResponse> {
            public const string Command = "unresolvedImports";

            public override string command => Command;

            public int fileId;
        }

        public sealed class UnresolvedImportsResponse : Response {
            public int version;
            public UnresolvedImport[] unresolved;
        }

        public sealed class UnresolvedImport {
            public string name;
            public int startIndex, endIndex, startLine, endLine, startColumn, endColumn;
        }

        public sealed class FileChangedResponse : Response {
#if DEBUG
            public string newCode;
#endif
        }

        public sealed class ChangeInfo {
            public string newText;
            public int start;
            public int length;
        }

        public class EnqueueFileResponse : Response {
            public int fileId;
        }

        public sealed class AddZipArchiveRequest : Request<Response> {
            public const string Command = "addZipArchive";
            public string archive;
            public override string command => Command;
        }

        public class TopLevelCompletionsRequest : Request<CompletionsResponse> {
            public const string Command = "topCompletions";

            public int fileId;
            public int location, column;
            public GetMemberOptions options;

            public override string command => Command;
        }

        public class GetModulesRequest : Request<CompletionsResponse> {
            public const string Command = "getModules";

            public int fileId;
            public bool topLevelOnly;

            public override string command => Command;
        }

        public class GetModuleMembers : Request<CompletionsResponse> {
            public const string Command = "getModuleMembers";

            public int fileId;
            public string[] package;
            public bool includeMembers;

            public override string command => Command;
        }

        public class CompletionsRequest : Request<CompletionsResponse> {
            public const string Command = "completions";

            public int fileId;
            public string text;
            public int location, column;
            public GetMemberOptions options;
            public bool forceCompletions;

            public override string command => Command;
        }

        public class SignaturesRequest : Request<SignaturesResponse> {
            public const string Command = "sigs";

            public override string command => Command;

            public string text;
            public int location, column;
            public int fileId;
        }

        public sealed class ModulesChangedEvent : Event {
            public const string Name = "modulesChanged";

            public override string name => Name;
        }

        public sealed class SignaturesResponse : Response {
            public Signature[] sigs;
        }

        public class Signature {
            public string name;
            public string doc;
            public Parameter[] parameters;
        }

        public class Parameter {
            public string name, defaultValue, doc, type;
            public bool optional;
            public Reference[] variables;
        }

        public class AnalysisCompleteEvent : Event {
            public const string Name = "analysisComplete";
            public int fileId;

            public override string name => Name;
        }

        public class CompletionsResponse : Response {
            public Completion[] completions;
        }

        public class Completion {
            public string name;
            public string completion;
            public PythonMemberType memberType;
        }

        public sealed class OptionsChangedEvent : Event {
            public const string Name = "optionsChanged";

            public Severity indentation_inconsistency_severity;
            public bool implicitProject;
            public int crossModuleAnalysisLimit;

            public override string name => Name;
        }

        public sealed class SetCommentTaskTokens : Event {
            public const string Name = "setCommentTaskTokens";

            public Dictionary<string, TaskPriority> tokens;

            public override string name => Name;
        }

        public sealed class TaskItem : Error {
            public TaskPriority priority;
            public TaskCategory category;
            public bool squiggle;
        }

        public enum TaskPriority {
            high,
            normal,
            low
        }

        public enum TaskCategory {
            buildCompile,
            comments
        }



        public class Reference {
            public string kind; // definition, reference, value
            public string expr;
            public string file;
            public int line, column;

        }

        public sealed class AnalyzeExpressionRequest : Request<AnalyzeExpressionResponse> {
            public const string Command = "findDefs";

            public int fileId;
            public string expr;
            public int line, column, index;

            public override string command => Command;
        }

        public sealed class AnalyzeExpressionResponse : Response {
            public Reference[] variables;
            /// <summary>
            /// The private prefix for the member if defined inside a class with name mangling.
            /// </summary>
            public string privatePrefix;
            /// <summary>
            /// The plain member name, e.g. "member" in a statement like "container.member"
            /// </summary>
            public string memberName;
        }

        public sealed class OutlingRegionsRequest : Request<OutliningRegionsResponse> {
            public const string Command = "outlingRegions";
            public int fileId;

            public override string command => Command;
        }

        public sealed class OutliningRegionsResponse : Response {
            public int version;
            public OutliningTag[] tags;
        }

        public sealed class OutliningTag {
            public int headerIndex, startIndex, endIndex;
        }

        public sealed class NavigationRequest : Request<NavigationResponse> {
            public const string Command = "navigation";
            public int fileId;

            public override string command => Command;
        }

        public sealed class NavigationResponse : Response {
            public int version;
            public Navigation[] navigations;
        }

        public sealed class Navigation {
            public string name, type;
            public int startIndex, endIndex;
            public Navigation[] children;
        }
    }
}

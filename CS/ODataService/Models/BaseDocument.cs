using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using DevExpress.Xpo;

namespace WebApplication1.Models {

    [Persistent("BaseDocument")]
    public abstract class BaseDocument : XPLiteObject {
        public BaseDocument() { }
        public BaseDocument(Session session): base(session) { }

        int fID;
        [Key(true)]
        public int ID {
            get { return fID; }
            set { SetPropertyValue<int>(nameof(ID), ref fID, value); }
        }

        DateTime? fDate;
        [Indexed(Name = @"DocumentDate")]
        public DateTime? Date {
            get { return fDate; }
            set { SetPropertyValue<DateTime?>(nameof(Date), ref fDate, value); }
        }

        [Association(@"BaseDocumentLinkedDocuments")]
        public XPCollection<BaseDocument> LinkedDocuments { get { return GetCollection<BaseDocument>(nameof(LinkedDocuments)); } }

        BaseDocument fParentDocument;
        [Association(@"BaseDocumentLinkedDocuments")]
        [Persistent("ParentDocument")]
        public BaseDocument ParentDocument {
            get { return fParentDocument; }
            set { SetPropertyValue<BaseDocument>(nameof(BaseDocument), ref fParentDocument, value); }
        }
    }
}
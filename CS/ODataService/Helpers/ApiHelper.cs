using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Web.OData;
using DevExpress.Xpo;

namespace ODataService.Helpers {
    public static class ApiHelper {
        public static TEntity Patch<TEntity, TKey>(TKey key, Delta<TEntity> delta) where TEntity : class {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                TEntity existing = uow.GetObjectByKey<TEntity>(key);
                if(existing != null) {
                    delta.CopyChangedValues(existing);
                    uow.CommitChanges();
                }
                return existing;
            }
        }

        public static HttpStatusCode Delete<TEntity, TKey>(TKey key) {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                TEntity existing = uow.GetObjectByKey<TEntity>(key);
                if(existing == null) {
                    return HttpStatusCode.NotFound;
                }
                uow.Delete(existing);
                uow.CommitChanges();
                return HttpStatusCode.NoContent;
            }
        }

        public static HttpStatusCode CreateRef<TEntity, TKey>(HttpRequestMessage request, TKey key, string navigationProperty, Uri link) {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                TEntity entity = uow.GetObjectByKey<TEntity>(key);
                if(entity == null) {
                    return HttpStatusCode.NotFound;
                }
                var classInfo = uow.GetClassInfo<TEntity>();
                var memberInfo = classInfo.FindMember(navigationProperty);
                if(memberInfo == null) {
                    return HttpStatusCode.BadRequest;
                }
                object relatedKey = UriHelper.GetKeyFromUri<object>(request, link);
                if(memberInfo.IsAssociationList) {
                    var reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey);
                    var collection = (IList)memberInfo.GetValue(entity);
                    collection.Add(reference);
                } else if(memberInfo.ReferenceType != null) {
                    var reference = uow.GetObjectByKey(memberInfo.ReferenceType, relatedKey);
                    memberInfo.SetValue(entity, reference);
                } else {
                    return HttpStatusCode.BadRequest;
                }
                uow.CommitChanges();
                return HttpStatusCode.NoContent;
            }
        }

        public static HttpStatusCode DeleteRef<TEntity, TKey>(HttpRequestMessage request, TKey key, string navigationProperty, Uri link) {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                TEntity entity = uow.GetObjectByKey<TEntity>(key);
                if(entity == null) {
                    return HttpStatusCode.NotFound;
                }
                var classInfo = uow.GetClassInfo<TEntity>();
                var memberInfo = classInfo.FindMember(navigationProperty);
                if(memberInfo == null) {
                    return HttpStatusCode.BadRequest;
                }
                if(memberInfo.ReferenceType != null) {
                    memberInfo.SetValue(entity, null);
                } else {
                    return HttpStatusCode.BadRequest;
                }
                uow.CommitChanges();
                return HttpStatusCode.NoContent;
            }
        }

        public static HttpStatusCode DeleteRef<TEntity, TKey, TRelatedKey>(TKey key, TRelatedKey relatedKey, string navigationProperty) {
            using(UnitOfWork uow = ConnectionHelper.CreateSession()) {
                TEntity entity = uow.GetObjectByKey<TEntity>(key);
                if(entity == null) {
                    return HttpStatusCode.NotFound;
                }
                var classInfo = uow.GetClassInfo<TEntity>();
                var memberInfo = classInfo.FindMember(navigationProperty);
                if(memberInfo == null || !memberInfo.IsAssociationList) {
                    return HttpStatusCode.BadRequest;
                }
                var reference = uow.GetObjectByKey(memberInfo.CollectionElementType, relatedKey);
                var collection = (IList)memberInfo.GetValue(entity);
                collection.Remove(reference);
                uow.CommitChanges();
                return HttpStatusCode.NoContent;
            }
        }
    }
}
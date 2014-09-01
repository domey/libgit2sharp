﻿using System;
using System.Collections.Generic;
using LibGit2Sharp.Core;
using LibGit2Sharp.Core.Handles;

namespace LibGit2Sharp
{
    /// <summary>
    /// Exposes properties of a remote that can be updated.
    /// </summary>
    public class RemoteUpdater : IDisposable
    {
        private readonly UpdatingCollection<string> fetchRefSpecs;
        private readonly UpdatingCollection<string> pushRefSpecs;
        private readonly RemoteSafeHandle remoteHandle;

        /// <summary>
        /// Needed for mocking purposes.
        /// </summary>
        protected RemoteUpdater()
        { }

        internal RemoteUpdater(Repository repo, Remote remote)
        {
            Ensure.ArgumentNotNull(repo, "repo");
            Ensure.ArgumentNotNull(remote, "remote");

            fetchRefSpecs = new UpdatingCollection<string>(GetFetchRefSpecs, SetFetchRefSpecs);
            pushRefSpecs = new UpdatingCollection<string>(GetPushRefSpecs, SetPushRefSpecs);

            remoteHandle = Proxy.git_remote_load(repo.Handle, remote.Name, true);
        }

        private IEnumerable<string> GetFetchRefSpecs()
        {
            return Proxy.git_remote_get_fetch_refspecs(remoteHandle);
        }

        private void SetFetchRefSpecs(IEnumerable<string> value)
        {
            Proxy.git_remote_set_fetch_refspecs(remoteHandle, value);
            Proxy.git_remote_save(remoteHandle);
        }

        private IEnumerable<string> GetPushRefSpecs()
        {
            return Proxy.git_remote_get_push_refspecs(remoteHandle);
        }

        private void SetPushRefSpecs(IEnumerable<string> value)
        {
            Proxy.git_remote_set_push_refspecs(remoteHandle, value);
            Proxy.git_remote_save(remoteHandle);
        }

        /// <summary>
        /// Set the default TagFetchMode value for the remote.
        /// </summary>
        public virtual TagFetchMode TagFetchMode
        {
            set
            {
                Proxy.git_remote_set_autotag(remoteHandle, value);
                Proxy.git_remote_save(remoteHandle);
            }
        }

        /// <summary>
        /// Sets the url defined for this <see cref="Remote"/>
        /// </summary>
        public virtual string Url
        {
            set
            {
                Proxy.git_remote_set_url(remoteHandle, value);
                Proxy.git_remote_save(remoteHandle);
            }
        }

        /// <summary>
        /// Sets the list of <see cref="RefSpec"/>s defined for this <see cref="Remote"/> that are intended to
        /// be used during a Fetch operation
        /// </summary>
        public virtual ICollection<string> FetchRefSpecs
        {
            get { return fetchRefSpecs; }
            set { fetchRefSpecs.ReplaceAll(value); }
        }

        /// <summary>
        /// Sets or gets the list of <see cref="RefSpec"/>s defined for this <see cref="Remote"/> that are intended to
        /// be used during a Push operation
        /// </summary>
        public virtual ICollection<string> PushRefSpecs
        {
            get { return pushRefSpecs; }
            set { pushRefSpecs.ReplaceAll(value); }
        }

        private class UpdatingCollection<T> : ICollection<T>
        {
            private readonly Lazy<List<T>> list;
            private readonly Action<IEnumerable<T>> setter;

            public UpdatingCollection(Func<IEnumerable<T>> getter,
                Action<IEnumerable<T>> setter)
            {
                list = new Lazy<List<T>>(() => new List<T>(getter()));
                this.setter = setter;
            }

            public void Add(T item)
            {
                list.Value.Add(item);
                Save();
            }

            public void Clear()
            {
                list.Value.Clear();
                Save();
            }

            public bool Contains(T item)
            {
                return list.Value.Contains(item);
            }

            public void CopyTo(T[] array, int arrayIndex)
            {
                list.Value.CopyTo(array, arrayIndex);
            }

            public int Count
            {
                get { return list.Value.Count; }
            }

            public bool IsReadOnly
            {
                get { return false; }
            }

            public bool Remove(T item)
            {
                if (!list.Value.Remove(item))
                {
                    return false;
                }

                Save();
                return true;
            }

            public IEnumerator<T> GetEnumerator()
            {
                return list.Value.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return list.Value.GetEnumerator();
            }

            public void ReplaceAll(IEnumerable<T> newValues)
            {
                Ensure.ArgumentNotNull(newValues, "newValues");
                list.Value.Clear();
                list.Value.AddRange(newValues);
                Save();
            }

            private void Save()
            {
                setter(list.Value);
            }
        }

        public void Dispose()
        {
            remoteHandle.Dispose();
        }
    }
}

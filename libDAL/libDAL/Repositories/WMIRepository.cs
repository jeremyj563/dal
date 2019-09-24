using System;
using System.Collections.Generic;

namespace DataRepositories
{
    public class WMIRepository //: IDataRepository
    {
        public WMIRepository()
        {
        }

        #region Public Methods

        public void New<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Get<T>(string cmd, (string, object)[] @params = null) where T : new()
        {
            throw new NotImplementedException();
        }

        public void Edit<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        public void Remove<T>(string cmd, T record) where T : new()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}

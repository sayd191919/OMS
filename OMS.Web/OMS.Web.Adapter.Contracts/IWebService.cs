﻿namespace OMS.Web.Adapter.Contracts
{
    using System.ServiceModel;
    using System.Collections.Generic;
    using OMS.Web.UI.Models.ViewModels;

    [ServiceContract]
    public interface IWebService
    {
        [OperationContract]
        void UpdateGraph(List<Node> nodes, List<Relation> relations);
    }
}

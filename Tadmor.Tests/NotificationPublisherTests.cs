using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using Tadmor.Notifications.Interfaces;
using Tadmor.Notifications.Services;

namespace Tadmor.Tests
{
    [TestClass]
    public class NotificationPublisherTests
    {
        private IServiceScope _scope;
        private IServiceScopeFactory _serviceScopeFactory;
        private NotificationPublisherFactory _sut;
        private INotificationHandler<string> _notificationHandler1;
        private INotificationHandler<string> _notificationHandler2;

        [TestInitialize]
        public void Initialize()
        {
            _scope = Substitute.For<IServiceScope>(); 
            _scope.ServiceProvider.GetService(typeof(INotificationPublisher)).Returns(new NotificationPublisher(_scope));
            _notificationHandler1 = Substitute.For<INotificationHandler<string>>();
            _notificationHandler2 = Substitute.For<INotificationHandler<string>>();
            _scope.ServiceProvider.GetService(typeof(IEnumerable<INotificationHandler<string>>)).Returns(new[]{_notificationHandler1, _notificationHandler2});
            _serviceScopeFactory = Substitute.For<IServiceScopeFactory>();
            _serviceScopeFactory.CreateScope().Returns(_scope);
            _sut = new NotificationPublisherFactory(_serviceScopeFactory);
        }

        [TestMethod]
        public async Task NotificationPublisher_And_Factory_Works()
        {
            var notificationPublisher = _sut.Create();
            var notification = "notification";
            await notificationPublisher.PublishAsync(notification, CancellationToken.None);
            await _notificationHandler1.Received().HandleAsync(notification, CancellationToken.None);
            await _notificationHandler2.Received().HandleAsync(notification, CancellationToken.None);
            notificationPublisher.Dispose();
            _scope.Received().Dispose();
        }
    }
}
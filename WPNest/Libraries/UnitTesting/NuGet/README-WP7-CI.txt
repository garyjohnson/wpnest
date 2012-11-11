The following code must be placed in the Loaded event of your startup Page:


	var testPage = UnitTestSystem.CreateTestPage() as IMobileTestPage;

	BackKeyPress += (x, xe) => xe.Cancel = testPage.NavigateBack();
	(Application.Current.RootVisual as PhoneApplicationFrame).Content = testPage;     

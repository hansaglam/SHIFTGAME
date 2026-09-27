#import <UIKit/UIKit.h>
// Called synchronously from Unity main thread. Never queue stale feedback on resume.
extern "C" void ShiftImpact(int style)
{
    if (![NSThread isMainThread] || [UIApplication sharedApplication].applicationState != UIApplicationStateActive) return;
    if (@available(iOS 10.0, *))
    {
        static UIImpactFeedbackGenerator *light;
        static UIImpactFeedbackGenerator *medium;
        static UIImpactFeedbackGenerator *strong;
        if (!light) light = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleLight];
        if (!medium) medium = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleMedium];
        if (!strong) strong = [[UIImpactFeedbackGenerator alloc] initWithStyle:UIImpactFeedbackStyleHeavy];
        UIImpactFeedbackGenerator *generator = style == 0 ? light : style == 1 ? medium : strong;
        [generator impactOccurred];
    }
}

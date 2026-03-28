import { Cookie, ArrowLeft } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import styles from './PolicyPage.module.css'

export default function CookiePolicyPage() {
  const navigate = useNavigate()

  return (
    <main className={styles.main}>
      <button className={styles.back} onClick={() => navigate('/')}>
        <ArrowLeft size={16} /> Back to Home
      </button>

      <div className={styles.header}>
        <div className={styles.icon}>
          <Cookie size={32} />
        </div>
        <h1 className={styles.title}>Cookie Policy</h1>
        <p className={styles.updated}>Last Updated: March 28, 2026</p>
      </div>

      <div className={styles.content}>
        <section>
          <h2>1. What Are Cookies?</h2>
          <p>
            Cookies are small text files that are placed on your device when you visit our website. 
            They help us provide you with a better experience by remembering your preferences and 
            understanding how you use our service.
          </p>
        </section>

        <section>
          <h2>2. How We Use Cookies</h2>
          <p>
            We use cookies and similar technologies for the following purposes:
          </p>
          <ul>
            <li>To remember your preferences (theme, language settings)</li>
            <li>To keep you logged in and maintain your session</li>
            <li>To understand how you interact with our service</li>
            <li>To improve our service and user experience</li>
            <li>To remember your cookie consent preferences</li>
          </ul>
        </section>

        <section>
          <h2>3. Types of Cookies We Use</h2>

          <h3>3.1 Essential Cookies (Required)</h3>
          <p>
            These cookies are necessary for the website to function properly. They enable core 
            functionality such as security, network management, and accessibility. You cannot 
            opt-out of these cookies.
          </p>
          <ul>
            <li><strong>cookieConsent:</strong> Stores your cookie consent preference</li>
            <li><strong>Session cookies:</strong> Maintain your session and authentication</li>
          </ul>

          <h3>3.2 Functional Cookies (Optional)</h3>
          <p>
            These cookies enable enhanced functionality and personalization, such as remembering 
            your theme preference (light/dark mode) and other settings.
          </p>
          <ul>
            <li><strong>theme:</strong> Remembers your preferred color theme</li>
            <li><strong>User preferences:</strong> Stores your customization choices</li>
          </ul>

          <h3>3.3 Analytics Cookies (Optional)</h3>
          <p>
            These cookies help us understand how visitors interact with our website by collecting 
            and reporting information anonymously. This helps us improve our service.
          </p>
          <ul>
            <li><strong>Usage statistics:</strong> Anonymous data about page views and interactions</li>
            <li><strong>Performance metrics:</strong> Information about site performance</li>
          </ul>
        </section>

        <section>
          <h2>4. Third-Party Cookies</h2>
          <p>
            We may use third-party services that set cookies on your device. These services are 
            carefully selected and comply with GDPR requirements. Third-party cookies may include:
          </p>
          <ul>
            <li>Analytics providers (e.g., Google Analytics)</li>
            <li>Content delivery networks (CDNs)</li>
            <li>Security and fraud prevention services</li>
          </ul>
        </section>

        <section>
          <h2>5. Cookie Duration</h2>
          <p>Cookies may be either:</p>
          <ul>
            <li><strong>Session Cookies:</strong> Temporary cookies that expire when you close your browser</li>
            <li><strong>Persistent Cookies:</strong> Cookies that remain on your device for a set period or until you delete them</li>
          </ul>
        </section>

        <section>
          <h2>6. Managing Your Cookie Preferences</h2>
          <p>
            You have the right to accept or reject non-essential cookies. You can manage your 
            cookie preferences in the following ways:
          </p>

          <h3>6.1 Cookie Consent Banner</h3>
          <p>
            When you first visit our website, you'll see a cookie consent banner where you can 
            choose to accept all cookies or reject non-essential cookies.
          </p>

          <h3>6.2 Browser Settings</h3>
          <p>
            Most web browsers allow you to control cookies through their settings. You can:
          </p>
          <ul>
            <li>View what cookies are stored and delete them individually</li>
            <li>Block third-party cookies</li>
            <li>Block all cookies from specific websites</li>
            <li>Delete all cookies when you close your browser</li>
          </ul>

          <h3>6.3 Browser-Specific Instructions</h3>
          <ul>
            <li><strong>Chrome:</strong> Settings → Privacy and security → Cookies and other site data</li>
            <li><strong>Firefox:</strong> Settings → Privacy & Security → Cookies and Site Data</li>
            <li><strong>Safari:</strong> Preferences → Privacy → Manage Website Data</li>
            <li><strong>Edge:</strong> Settings → Cookies and site permissions → Cookies and site data</li>
          </ul>
        </section>

        <section>
          <h2>7. Impact of Disabling Cookies</h2>
          <p>
            If you choose to disable cookies, some features of our website may not function properly:
          </p>
          <ul>
            <li>You may need to re-enter your preferences each visit</li>
            <li>Some personalization features may not work</li>
            <li>Your theme preference won't be saved</li>
          </ul>
          <p>
            Essential cookies cannot be disabled as they are necessary for the website to function.
          </p>
        </section>

        <section>
          <h2>8. Local Storage</h2>
          <p>
            In addition to cookies, we use browser local storage to save:
          </p>
          <ul>
            <li>Your theme preference (light/dark mode)</li>
            <li>Your cookie consent choice</li>
            <li>Application settings and preferences</li>
          </ul>
          <p>
            Local storage data remains on your device until you clear it through your browser settings.
          </p>
        </section>

        <section>
          <h2>9. Updates to This Policy</h2>
          <p>
            We may update this Cookie Policy from time to time to reflect changes in our practices 
            or for legal, operational, or regulatory reasons. The "Last Updated" date at the top 
            indicates when the policy was last revised.
          </p>
        </section>

        <section>
          <h2>10. Contact Us</h2>
          <p>
            If you have any questions about our use of cookies or this Cookie Policy, please contact us at:
          </p>
          <p className={styles.contact}>
            Email: rein.codeux@gmail.com<br />         
          </p>
        </section>

        <section>
          <h2>11. Your Rights</h2>
          <p>
            Under GDPR and other data protection laws, you have rights regarding cookies and 
            tracking technologies:
          </p>
          <ul>
            <li>Right to be informed about cookies we use</li>
            <li>Right to accept or reject non-essential cookies</li>
            <li>Right to withdraw consent at any time</li>
            <li>Right to access information about cookies stored on your device</li>
          </ul>
        </section>
      </div>
    </main>
  )
}

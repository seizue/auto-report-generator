import { Shield, ArrowLeft } from 'lucide-react'
import { useNavigate } from 'react-router-dom'
import styles from './PolicyPage.module.css'

export default function PrivacyPolicyPage() {
  const navigate = useNavigate()

  return (
    <main className={styles.main}>
      <button className={styles.back} onClick={() => navigate('/')}>
        <ArrowLeft size={16} /> Back to Home
      </button>

      <div className={styles.header}>
        <div className={styles.icon}>
          <Shield size={32} />
        </div>
        <h1 className={styles.title}>Privacy Policy</h1>
        <p className={styles.updated}>Last Updated: March 28, 2026</p>
      </div>

      <div className={styles.content}>
        <section>
          <h2>1. Introduction</h2>
          <p>
            Welcome to AutoReport ("we," "our," or "us"). We are committed to protecting your privacy 
            and ensuring transparency about how we collect, use, and protect your personal information. 
            This Privacy Policy explains our practices in accordance with the General Data Protection 
            Regulation (GDPR) and other applicable data protection laws.
          </p>
        </section>

        <section>
          <h2>2. Data Controller</h2>
          <p>
            AutoReport is the data controller responsible for your personal information. 
            For any privacy-related questions or requests, please contact us at: privacy@autoreport.com
          </p>
        </section>

        <section>
          <h2>3. Information We Collect</h2>
          <h3>3.1 Information You Provide</h3>
          <ul>
            <li>Name and department information when creating reports</li>
            <li>Work-related data including tasks, notes, and time entries</li>
            <li>Any text or images you upload for OCR processing</li>
          </ul>

          <h3>3.2 Automatically Collected Information</h3>
          <ul>
            <li>Browser type and version</li>
            <li>Device information</li>
            <li>Usage data and preferences (theme, settings)</li>
            <li>Cookies and similar tracking technologies</li>
          </ul>
        </section>

        <section>
          <h2>4. How We Use Your Information</h2>
          <p>We use your personal information for the following purposes:</p>
          <ul>
            <li>To generate and format reports based on your input</li>
            <li>To provide OCR (Optical Character Recognition) services</li>
            <li>To save and retrieve your report history</li>
            <li>To improve our services and user experience</li>
            <li>To maintain security and prevent fraud</li>
          </ul>
        </section>

        <section>
          <h2>5. Legal Basis for Processing (GDPR)</h2>
          <p>We process your personal data based on:</p>
          <ul>
            <li><strong>Consent:</strong> You have given clear consent for us to process your data</li>
            <li><strong>Legitimate Interests:</strong> Processing is necessary for our legitimate interests</li>
            <li><strong>Legal Obligation:</strong> Processing is necessary to comply with the law</li>
          </ul>
        </section>

        <section>
          <h2>6. Data Retention</h2>
          <p>
            We automatically delete all reports and associated data after <strong>2 days</strong> from 
            the creation date. This ensures we only keep your data for as long as necessary and helps 
            us comply with data minimization principles under GDPR.
          </p>
          <p>
            You can also manually delete your reports at any time through the History page.
          </p>
        </section>

        <section>
          <h2>7. Your Rights Under GDPR</h2>
          <p>You have the following rights regarding your personal data:</p>
          <ul>
            <li><strong>Right to Access:</strong> Request copies of your personal data</li>
            <li><strong>Right to Rectification:</strong> Request correction of inaccurate data</li>
            <li><strong>Right to Erasure:</strong> Request deletion of your personal data</li>
            <li><strong>Right to Restrict Processing:</strong> Request limitation of data processing</li>
            <li><strong>Right to Data Portability:</strong> Receive your data in a structured format</li>
            <li><strong>Right to Object:</strong> Object to processing of your personal data</li>
            <li><strong>Right to Withdraw Consent:</strong> Withdraw consent at any time</li>
          </ul>
        </section>

        <section>
          <h2>8. Data Security</h2>
          <p>
            We implement appropriate technical and organizational measures to protect your personal 
            information against unauthorized access, alteration, disclosure, or destruction. However, 
            no method of transmission over the internet is 100% secure.
          </p>
        </section>

        <section>
          <h2>9. Third-Party Services</h2>
          <p>
            We do not share your personal information with third parties except as necessary to 
            provide our services or as required by law. Any third-party services we use are 
            GDPR-compliant and process data according to our instructions.
          </p>
        </section>

        <section>
          <h2>10. International Data Transfers</h2>
          <p>
            Your data is processed and stored locally. If we transfer data internationally, 
            we ensure appropriate safeguards are in place in accordance with GDPR requirements.
          </p>
        </section>

        <section>
          <h2>11. Children's Privacy</h2>
          <p>
            Our service is not intended for children under 16 years of age. We do not knowingly 
            collect personal information from children under 16.
          </p>
        </section>

        <section>
          <h2>12. Changes to This Policy</h2>
          <p>
            We may update this Privacy Policy from time to time. We will notify you of any changes 
            by posting the new policy on this page and updating the "Last Updated" date.
          </p>
        </section>

        <section>
          <h2>13. Contact Us</h2>
          <p>
            If you have any questions about this Privacy Policy or wish to exercise your rights, 
            please contact us at:
          </p>
          <p className={styles.contact}>
            Email: rein.codeux@gmail.com<br />
          </p>
        </section>

        <section>
          <h2>14. Supervisory Authority</h2>
          <p>
            If you believe we have not addressed your concerns adequately, you have the right to 
            lodge a complaint with your local data protection supervisory authority.
          </p>
        </section>
      </div>
    </main>
  )
}

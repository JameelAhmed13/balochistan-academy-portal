import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { useSettingsStore } from '@/stores/settings'

const routes = [
  // Public landing page
  { path: '/', name: 'Landing', component: () => import('@/views/HomeLanding.vue'), meta: { public: true } },
  { path: '/landing1', name: 'Landing1', component: () => import('@/views/LandingPage.vue'), meta: { public: true } },
  { path: '/landing2', name: 'Landing2', component: () => import('@/views/Landing2Page.vue'), meta: { public: true } },
  { path: '/landing3', name: 'Landing3', component: () => import('@/views/Landing3Page.vue'), meta: { public: true } },
  { path: '/landing4', name: 'Landing4', component: () => import('@/views/Landing4Page.vue'), meta: { public: true } },

  // Legal / policy pages (public)
  { path: '/privacy',      name: 'Privacy',      component: () => import('@/views/legal/LegalView.vue'), meta: { public: true, doc: 'privacy' } },
  { path: '/refund',       name: 'Refund',       component: () => import('@/views/legal/LegalView.vue'), meta: { public: true, doc: 'refund' } },
  { path: '/terms',        name: 'Terms',        component: () => import('@/views/legal/LegalView.vue'), meta: { public: true, doc: 'terms' } },
  { path: '/cancellation', name: 'Cancellation', component: () => import('@/views/legal/LegalView.vue'), meta: { public: true, doc: 'cancellation' } },

  // Auth
  { path: '/login', name: 'Login', component: () => import('@/views/auth/LoginView.vue'), meta: { guest: true } },

  // App shell (authenticated)
  {
    path: '/app',
    component: () => import('@/components/layout/AppShell.vue'),
    meta: { requiresAuth: true },
    children: [
      // Dashboard
      { path: '',              name: 'Home',         component: () => import('@/views/dashboard/HomeView.vue') },

      // Hub
      { path: 'hub',           name: 'Hub',          component: () => import('@/views/hub/HubView.vue') },

      // AI Tutor
      { path: 'ai-tutor',     name: 'AITutor',      meta: { requiresAiEnabled: true },   component: () => import('@/views/ai-tutor/AITutorView.vue') },
      { path: 'ai-tutor/:subject/chat',  name: 'AITutorChat',  meta: { requiresAiEnabled: true },   component: () => import('@/views/ai-tutor/ChatView.vue'), props: true },
      { path: 'ai-tutor/:subject/video', name: 'AITutorVideo', meta: { requiresVideoEnabled: true }, component: () => import('@/views/ai-tutor/VideoView.vue'), props: true },

      // Saathi AI companion
      { path: 'saathi',              name: 'Saathi',            meta: { requiresAiEnabled: true },   component: () => import('@/views/saathi/SaathiView.vue') },
      { path: 'report',              name: 'PreparationReport', component: () => import('@/views/reports/PreparationReportView.vue') },

      // Profile (any logged-in user) + subscription checkout
      { path: 'profile',             name: 'Profile',           component: () => import('@/views/profile/ProfileView.vue') },
      { path: 'checkout',            name: 'Checkout',          component: () => import('@/views/subscription/CheckoutView.vue') },

      // Preparation — layout wraps all sub-routes so sidebar stays persistent
      {
        path: 'preparation',
        component: () => import('@/views/preparation/PreparationLayout.vue'),
        children: [
          { path: '',        name: 'Preparation', component: () => import('@/views/preparation/PreparationView.vue') },
          { path: 'syllabus', name: 'MySyllabus', component: () => import('@/views/preparation/StudentSyllabusView.vue') },
          { path: ':bookId', name: 'BookOptions', component: () => import('@/views/preparation/PreparationView.vue'), props: true },
          { path: ':bookId/objective',       name: 'ObjectivePaper',  component: () => import('@/views/preparation/ObjectivePaperView.vue'), props: true },
          { path: ':bookId/subjective',      name: 'SubjectivePaper', component: () => import('@/views/preparation/SubjectivePaperView.vue'), props: true },
          // In-layout study panels (sidebar stays persistent — single-page workspace)
          { path: ':bookId/resources',       name: 'PrepResources',   component: () => import('@/views/preparation/panels/ResourcesPanel.vue'),   props: true },
          { path: ':bookId/videos',          name: 'PrepVideos',      component: () => import('@/views/preparation/panels/VideosPanel.vue'),      props: true },
          { path: ':bookId/keynotes',        name: 'PrepKeyNotes',    component: () => import('@/views/preparation/panels/KeyNotesPanel.vue'),    props: true },
          { path: ':bookId/simulations',     name: 'PrepSimulations', component: () => import('@/views/preparation/panels/SimulationsPanel.vue'), props: true },
          { path: ':bookId/vlab',            name: 'PrepVirtualLab',  component: () => import('@/views/preparation/panels/VirtualLabPanel.vue'),  props: true },
          { path: ':bookId/pastpapers',      name: 'PrepPastPapers',  component: () => import('@/views/preparation/panels/PastPapersPanel.vue'),  props: true },
          { path: ':bookId/test/objective',  name: 'ObjectiveTest',   component: () => import('@/views/preparation/ObjectiveTestView.vue'), props: true },
          { path: ':bookId/test/subjective', name: 'SubjectiveTest',  component: () => import('@/views/preparation/SubjectiveTestView.vue'), props: true },
        ],
      },

      // Self Test flow
      { path: 'self-test',                      name: 'SelfTest',       component: () => import('@/views/self-test/SelfTestView.vue') },
      { path: 'self-test/follow-up',            name: 'SelfTestFollowUp', component: () => import('@/views/self-test/SelfTestFollowUpView.vue') },
      { path: 'self-test/:bookId/config',       name: 'SelfTestConfig', component: () => import('@/views/self-test/SelfTestConfigView.vue'), props: true },
      { path: 'self-test/:bookId/take',         name: 'SelfTestTaking', component: () => import('@/views/self-test/SelfTestTakingView.vue'), props: true },
      { path: 'self-test/:bookId/result',       name: 'SelfTestResult', component: () => import('@/views/self-test/SelfTestResultView.vue'), props: true },

      // Objective Tests
      { path: 'objective-tests',                name: 'ObjectiveTests',       component: () => import('@/views/tests/ObjectiveTestsListView.vue') },
      { path: 'objective-tests/:id/result',     name: 'ObjectiveTestResult',  component: () => import('@/views/tests/ObjectiveTestResultView.vue'), props: true },

      // Parent Tests
      { path: 'parent-tests',                   name: 'ParentTests',          component: () => import('@/views/tests/ParentTestsView.vue') },
      { path: 'parent-tests/follow-up',         name: 'ParentTestFollowUp',   component: () => import('@/views/tests/ParentTestFollowUpView.vue') },
      { path: 'parent-tests/:id/take',          name: 'ParentTestTaking',     component: () => import('@/views/tests/ParentTestTakingView.vue'), props: true },
      { path: 'parent-tests/:id/result',        name: 'ParentTestResult',     component: () => import('@/views/tests/ObjectiveTestResultView.vue'), props: true },

      // Daily Tests
      { path: 'daily-tests',                    name: 'DailyTests',           component: () => import('@/views/daily-tests/DailyTestsView.vue') },
      { path: 'daily-tests/combined',           name: 'DailyCombinedTest',    component: () => import('@/views/daily-tests/DailyCombinedTestView.vue') },
      { path: 'daily-tests/result/:id',         name: 'DailyTestResult',      component: () => import('@/views/daily-tests/DailyTestResultView.vue'), props: true },

      // Competition / Monthly / Challenge / Weekly / Leaderboard
      { path: 'competition',                    name: 'Competition',          component: () => import('@/views/competition/CompetitionView.vue') },
      { path: 'competition/monthly-test',       name: 'MonthlyTest',          component: () => import('@/views/competition/MonthlyTestView.vue') },
      { path: 'competition/result/:id',         name: 'MonthlyTestResult',    component: () => import('@/views/competition/MonthlyTestResultView.vue'), props: true },
      { path: 'competition/challenge',          name: 'ChallengeQuiz',        component: () => import('@/views/competition/ChallengeQuizView.vue') },
      { path: 'competition/weekly',             name: 'WeeklyQuiz',           component: () => import('@/views/competition/WeeklyQuizView.vue') },
      { path: 'competition/leaderboard',        name: 'Leaderboard',          component: () => import('@/views/competition/LeaderboardView.vue') },

      // Video Lectures
      { path: 'video',                          name: 'VideoLectures',        component: () => import('@/views/video/VideoLecturesView.vue') },
      { path: 'video/course/:courseId',         name: 'VideoCourse',          component: () => import('@/views/video/VideoCourseView.vue'), props: true },
      { path: 'video/player/:courseId/:lessonId', name: 'VideoPlayer',        component: () => import('@/views/video/VideoPlayerView.vue'), props: true },

      // Tools & Features
      { path: 'calculator',        name: 'Calculator',       component: () => import('@/views/tools/CalculatorView.vue') },
      { path: 'periodic-table',    name: 'PeriodicTable',    component: () => import('@/views/tools/PeriodicTableView.vue') },
      { path: 'log-table',         name: 'LogTable',         component: () => import('@/views/tools/LogTableView.vue') },
      { path: 'dictionary',        name: 'Dictionary',       component: () => import('@/views/tools/DictionaryView.vue') },
      { path: 'time-table',        name: 'TimeTable',        component: () => import('@/views/tools/TimeTableView.vue') },
      { path: 'smart-notes',       name: 'SmartNotes',       component: () => import('@/views/tools/SmartNotesView.vue') },
      { path: 'full-prep',         name: 'FullPrep',         component: () => import('@/views/tools/FullPrepView.vue') },
      { path: 'global-library',    name: 'GlobalLibrary',    component: () => import('@/views/tools/GlobalLibraryView.vue') },
      { path: 'learn-coding',      name: 'LearnCoding',      component: () => import('@/views/tools/LearnCodingView.vue') },
      { path: 'puzzle-games',      name: 'PuzzleGames',      component: () => import('@/views/tools/PuzzleGamesView.vue') },
      { path: 'online-classes',    name: 'OnlineClasses',    component: () => import('@/views/tools/OnlineClassesView.vue') },
      { path: 'career-counselor',  name: 'CareerCounselor',  component: () => import('@/views/tools/CareerCounselorView.vue') },
      { path: 'join-forces',       name: 'JoinForces',       component: () => import('@/views/tools/JoinForcesView.vue') },
      { path: 'typing-master',     name: 'TypingMaster',     component: () => import('@/views/tools/TypingMasterView.vue') },
      { path: 'messages',          name: 'Messages',         component: () => import('@/views/tools/MessagesView.vue') },
      { path: 'slos-mapping',      name: 'SLOsMapping',      component: () => import('@/views/tools/SLOsMappingView.vue') },
      { path: 'mdcat',             name: 'MDCAT',            component: () => import('@/views/tools/MDCATView.vue') },
      { path: 'etea',              name: 'ETEA',             component: () => import('@/views/tools/ETEAView.vue') },
      { path: 'conceptual',        name: 'Conceptual',       component: () => import('@/views/tools/ConceptualView.vue') },
      { path: 'experiments',       name: 'Experiments',      component: () => import('@/views/tools/ExperimentsView.vue') },
      { path: 'general-knowledge', name: 'GeneralKnowledge', component: () => import('@/views/tools/GeneralKnowledgeView.vue') },
      { path: 'simulations',       name: 'Simulations',      component: () => import('@/views/tools/SimulationsView.vue') },
      { path: 'study-notebook',   name: 'StudyNotebook',   component: () => import('@/views/tools/StudyNotebookView.vue') },
      { path: 'ebooks',            name: 'EBooks',           component: () => import('@/views/tools/EBooksView.vue') },
      { path: 'mcqs-bank',         name: 'MCQsBank',         component: () => import('@/views/tools/MCQsBankView.vue') },
      { path: 'past-papers',       name: 'PastPapers',       component: () => import('@/views/tools/PastPapersView.vue') },
      { path: 'progress-tracking', name: 'ProgressTracking', component: () => import('@/views/tools/ProgressTrackingView.vue') },
      { path: 'chatgpt',           redirect: '/app/saathi' },

      // Core app routes
      { path: 'notifications', name: 'MyNotifications', component: () => import('@/views/notifications/MyNotificationsView.vue') },
      { path: 'coins',        name: 'Coins',        component: () => import('@/views/coins/CoinsView.vue') },
      { path: 'reports',      name: 'Reports',      component: () => import('@/views/reports/ReportsView.vue') },
      { path: 'complaints',   name: 'Complaints',   component: () => import('@/views/complaints/ComplaintsView.vue') },

    ],
  },

  // Admin — dedicated shell with a Udemy-style top mega-menu (no sidebar)
  {
    path: '/app/admin',
    component: () => import('@/components/admin/AdminShell.vue'),
    meta: { requiresAuth: true, requiresAdmin: true },
    children: [
      { path: '',              name: 'Admin',              component: () => import('@/views/admin/AdminView.vue') },
      { path: 'grades',        name: 'AdminGrades',        component: () => import('@/views/admin/GradesView.vue') },
      { path: 'bands',         name: 'AdminBands',         component: () => import('@/views/admin/BandsView.vue') },
      { path: 'mediums',       name: 'AdminMediums',       component: () => import('@/views/admin/MediumsView.vue') },
      { path: 'syllabus',      name: 'AdminSyllabus',      component: () => import('@/views/admin/SyllabusView.vue') },
      { path: 'tutors',        name: 'AdminTutors',        component: () => import('@/views/admin/TutorsView.vue') },
      { path: 'content',       name: 'AdminContent',       component: () => import('@/views/admin/ContentView.vue') },
      { path: 'content/:subjectId/books', name: 'AdminBooks', component: () => import('@/views/admin/BooksView.vue'), props: true },
      { path: 'content/:subjectId/books/:bookId/units', name: 'AdminBookUnits', component: () => import('@/views/admin/BookUnitsView.vue'), props: true },
      { path: 'content/:subjectId/books/:bookId/units/:unitId/topics', name: 'AdminUnitTopics', component: () => import('@/views/admin/UnitTopicsView.vue'), props: true },
      { path: 'questions',     name: 'AdminQuestions',     component: () => import('@/views/admin/QuestionsView.vue') },
      { path: 'tests',         name: 'AdminTests',         component: () => import('@/views/admin/TestsView.vue') },
      { path: 'upload-assessments', name: 'AdminUploadAssessments', component: () => import('@/views/assessments/AssessmentUploadView.vue') },
      { path: 'profile',       name: 'AdminProfile',       component: () => import('@/views/profile/ProfileView.vue') },
      { path: 'users',         name: 'AdminUsers',         component: () => import('@/views/admin/UsersView.vue') },
      { path: 'roles',         name: 'AdminRoles',         component: () => import('@/views/admin/RolesView.vue') },
      { path: 'institutes',    name: 'AdminInstitutes',    component: () => import('@/views/admin/InstitutesView.vue') },
      { path: 'coins',         name: 'AdminCoins',         component: () => import('@/views/admin/CoinsAdminView.vue') },
      { path: 'analytics',     name: 'AdminAnalytics',     component: () => import('@/views/admin/AnalyticsView.vue') },
      { path: 'notifications', name: 'AdminNotifications', component: () => import('@/views/admin/NotificationsView.vue') },
      { path: 'notifications/inbox', name: 'AdminNotificationsInbox', component: () => import('@/views/notifications/MyNotificationsView.vue') },
      { path: 'settings',      name: 'AdminSettings',      component: () => import('@/views/admin/SettingsView.vue') },
      { path: 'complaints',    name: 'AdminComplaints',    component: () => import('@/views/admin/ComplaintsAdminView.vue') },
    ],
  },
  { path: '/app/select-grade', name: 'SelectGrade', component: () => import('@/views/auth/SelectGradeView.vue'), meta: { requiresAuth: true } },
  { path: '/:pathMatch(.*)*', redirect: '/' },
]

const router = createRouter({
  history: createWebHistory(),
  routes,
  scrollBehavior(to) {
    // Footer "Platform" links navigate to a specific landing section
    if (to.hash) return { el: to.hash, top: 80, behavior: 'smooth' }
    return { top: 0 }
  },
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.requiresAuth && !auth.isLoggedIn) return { name: 'Login' }
  if (to.meta.guest && auth.isLoggedIn) return { name: 'Home' }
  if (to.meta.requiresAdmin && auth.user?.role !== 'admin') return { name: 'Home' }
  // Students must have a grade before entering the student app (admins exempt).
  if (
    auth.isLoggedIn &&
    auth.user?.role === 'student' &&
    !auth.hasGrade &&
    to.path.startsWith('/app') &&
    to.name !== 'SelectGrade'
  ) {
    return { name: 'SelectGrade' }
  }
  // Feature flag guards — admins bypass so they can always test
  if (auth.user?.role !== 'admin') {
    const settings = useSettingsStore()
    if (to.meta.requiresAiEnabled    && settings.get('ai_tutor_enabled',    'true') === 'false') return { name: 'Home' }
    if (to.meta.requiresVideoEnabled && settings.get('video_lessons_enabled', 'true') === 'false') return { name: 'Home' }
  }
})

export default router

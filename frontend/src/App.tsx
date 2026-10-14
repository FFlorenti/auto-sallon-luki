import { lazy, Suspense, type ReactNode } from 'react'
import { createBrowserRouter, RouterProvider } from 'react-router'
import { Spinner } from '@/components/Feedback'
import { Layout } from '@/components/Layout'
import { CarDetailPage } from '@/pages/CarDetailPage'
import { CarListPage } from '@/pages/CarListPage'
import { ContactPage } from '@/pages/ContactPage'
import { FavoritesPage } from '@/pages/FavoritesPage'
import { NotFoundPage } from '@/pages/NotFoundPage'

const AdminLayout = lazy(() => import('@/pages/admin/AdminLayout').then((m) => ({ default: m.AdminLayout })))
const LoginPage = lazy(() => import('@/pages/admin/LoginPage').then((m) => ({ default: m.LoginPage })))
const DashboardPage = lazy(() => import('@/pages/admin/DashboardPage').then((m) => ({ default: m.DashboardPage })))
const AdminCarsPage = lazy(() => import('@/pages/admin/AdminCarsPage').then((m) => ({ default: m.AdminCarsPage })))
const AdminCarFormPage = lazy(() => import('@/pages/admin/AdminCarFormPage').then((m) => ({ default: m.AdminCarFormPage })))
const AdminInquiriesPage = lazy(() => import('@/pages/admin/AdminInquiriesPage').then((m) => ({ default: m.AdminInquiriesPage })))

const load = (element: ReactNode) => <Suspense fallback={<Spinner />}>{element}</Suspense>

const router = createBrowserRouter([
  {
    element: <Layout />,
    children: [
      { path: '/', element: <CarListPage /> },
      { path: '/cars/:id', element: <CarDetailPage /> },
      { path: '/favorites', element: <FavoritesPage /> },
      { path: '/contact', element: <ContactPage /> },
      { path: '*', element: <NotFoundPage /> },
    ],
  },
  { path: '/admin/login', element: load(<LoginPage />) },
  {
    path: '/admin',
    element: load(<AdminLayout />),
    children: [
      { index: true, element: load(<DashboardPage />) },
      { path: 'cars', element: load(<AdminCarsPage />) },
      { path: 'cars/new', element: load(<AdminCarFormPage />) },
      { path: 'cars/:id', element: load(<AdminCarFormPage />) },
      { path: 'inquiries', element: load(<AdminInquiriesPage />) },
    ],
  },
])

export default function App() {
  return <RouterProvider router={router} />
}

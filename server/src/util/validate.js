// Tiny zod wrapper → throws a tagged error the global handler turns into 400.
export function parse(schema, data) {
  const r = schema.safeParse(data)
  if (!r.success) {
    const err = new Error('Validation failed')
    err.type = 'validation'
    err.issues = r.error.issues.map((i) => ({ path: i.path.join('.'), message: i.message }))
    throw err
  }
  return r.data
}
